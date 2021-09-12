module App

open System
open Browser.Dom
open Feliz
open Feliz.MaterialUI
open Feliz.Router
open Fable.Form.Simple
open Elmish
open Elmish.React

#if DEBUG
open Elmish.Debug
open Elmish.HMR
#endif

module RouteNames =
    let [<Literal>] Home = "home"
    let [<Literal>] Login = "login"

type AppPage =
    | LoginPage
    | Home
with
    static member UrlToPage = function
        | [RouteNames.Home] -> AppPage.Home
        | [RouteNames.Login] | _ -> AppPage.LoginPage

    static member PageToUrl = function
        | AppPage.Home -> [RouteNames.Home], []
        | AppPage.LoginPage -> [RouteNames.Login], []

type LoginFormValues = {
    UserName : string
    Password : string
} with
    static member Init = { UserName = ""; Password = "" }


type LoginFormState = {
    LoginFormValues : Form.View.Model<LoginFormValues>
    Error : string option
} with
    static member Init =
        { LoginFormValues = LoginFormValues.Init |> Form.View.idle
          Error = None }

type AppProtectedState = {
    CurrentUser : string
}


type AppState = {
    Page : AppPage
    LoginFormState : LoginFormState
    ProtectedState : AppProtectedState option
} with
    static member Init (page: AppPage) =
        { Page = page
          LoginFormState = LoginFormState.Init
          ProtectedState = None }

type LoginFormMessage =
    | LoginFormChanged of Form.View.Model<LoginFormValues>
    | SubmitLogin of userName: string * password: string


type AppMessage =
    | LoginMessage of LoginFormMessage
    | Authenticate of userName: string
    | GoToPage of page: AppPage


let LoginForm : Form.Form<LoginFormValues, LoginFormMessage> =
    let userNameField =
        Form.textField {
            Parser = fun value ->
                if String.IsNullOrWhiteSpace value
                then Result.Error "Invalid user name"
                else Ok value
            Value = fun values -> values.UserName
            Update = fun newValue values -> { values with UserName = newValue }
            Error = fun e -> None
            Attributes = {
                Label = "User name"
                Placeholder = ""
            }
        }

    let passwordField =
        Form.passwordField {
            Parser = fun value ->
                if String.IsNullOrWhiteSpace value then
                    Result.Error "Password is required"
                else Ok value
            Value =
                fun values ->
                    values.Password
            Update =
                fun newValue values ->
                    { values with Password = newValue }
            Error =
                fun _ -> None
            Attributes =
                {
                    Label = "Password"
                    Placeholder = ""
                }
        }

    let onSubmit =
        fun userName password ->
            LoginFormMessage.SubmitLogin(userName, password)

    Form.succeed onSubmit
    |> Form.append userNameField
    |> Form.append passwordField


[<ReactComponent>]
let LoginFormView (state: LoginFormState) (dispatch: LoginFormMessage -> unit) =
    let dispatch = React.useCallback dispatch
    let formValues = React.useMemo ((fun () -> state.LoginFormValues), [| state.LoginFormValues |])
    Mui.container [
        container.maxWidth.xs
        container.children [
            Fable.Form.MaterialUI.renderMaterialUIForm
                [
                    grid.direction.column
                    grid.alignItems.stretch
                ]
                [
                    button.fullWidth true
                    button.variant.contained
                ]
                None
                {
                    Form.View.Dispatch = dispatch
                    Form.View.OnChange = LoginFormMessage.LoginFormChanged
                    Form.View.Action = "Log In"
                    Form.View.Validation = Form.View.Validation.ValidateOnBlur
                }
                LoginForm
                formValues
        ]
    ]


[<ReactComponent>]
let ProtectedView (state: AppProtectedState) (message: AppMessage -> unit) =
    Mui.typography [
        typography.variant.h4
        typography.align.center
        typography.children $"CurrentUser: {state.CurrentUser}"
    ]


[<ReactComponent>]
let AppBar (page: AppPage) (isLoggedIn: bool) =
    let pageTab (disabled: bool) (value: AppPage) (label: string) =
        Mui.tab [
            tab.value (AppPage.PageToUrl value |> Router.format)
            tab.label label
            tab.disabled disabled
        ]

    Mui.appBar [
        appBar.position.static'
        appBar.children [
            Mui.toolbar [
                toolbar.children [
                    Mui.tabs [
                        tabs.value (AppPage.PageToUrl page |> Router.format)

                        tabs.onChange (fun e (v: string) ->
                                            v |> Router.navigate)

                        tabs.children [
                            pageTab false AppPage.LoginPage "Log In"
                            pageTab
                                (not isLoggedIn)
                                AppPage.Home "Home"
                        ]
                    ]
                ]
            ]
        ]
    ]


[<ReactComponent>]
let AppView (state: AppState) (dispatch: AppMessage -> unit) =    
    React.router [
        router.onUrlChanged (AppPage.UrlToPage >> AppMessage.GoToPage >> dispatch)
        router.children [
            AppBar state.Page state.ProtectedState.IsSome
            Mui.container [
                match state.Page with
                | AppPage.LoginPage ->
                    LoginFormView (state.LoginFormState) (LoginMessage >> dispatch)

                | AppPage.Home ->
                    match state.ProtectedState with
                    | Some protectedState ->
                        ProtectedView protectedState dispatch
                    | None ->
                        Html.div [
                            Html.span (Html.text "Redirecting to Log In page...")
                            Mui.circularProgress [
                                circularProgress.variant.indeterminate
                            ]
                        ]
            ]
        ]
    ]



let LoginFormUpdate (message: LoginFormMessage) (state: LoginFormState) =
    match message with
    | LoginFormMessage.LoginFormChanged newModel ->
        { state with
            LoginFormValues = newModel },
        Cmd.none

    | LoginFormMessage.SubmitLogin(userName, password) ->
        { state with LoginFormValues = state.LoginFormValues |> Form.View.setLoading },
        async {
            do! Async.Sleep 1000
            return AppMessage.Authenticate userName
        } |> Cmd.OfAsync.result


let AppUpdate (message: AppMessage) (state: AppState) =
    match message with
    | AppMessage.Authenticate userName ->
        { state with
            Page = AppPage.Home
            ProtectedState = Some { CurrentUser = userName }
            LoginFormState = LoginFormState.Init
        },
        Cmd.navigate([RouteNames.Home], [])
    
    | AppMessage.LoginMessage loginMessage ->
        let updLoginState, loginCmd =
            LoginFormUpdate loginMessage state.LoginFormState
        { state with LoginFormState = updLoginState },
        loginCmd

    | AppMessage.GoToPage page ->
        match page with
        | AppPage.LoginPage ->
            { state with Page = AppPage.LoginPage },
            Cmd.none
        
        | AppPage.Home when state.ProtectedState.IsNone ->
            state,
            Cmd.navigate ([RouteNames.Login], [])

        | AppPage.Home ->
            { state with Page = AppPage.Home },
            Cmd.none
    

let AppInit (urlSegments: string list) =
    AppState.Init AppPage.LoginPage,
    urlSegments |> Router.format |> Cmd.navigate


let App (appElementId: string) =
    Program.mkProgram AppInit AppUpdate AppView
    #if DEBUG
    |> Program.withConsoleTrace
    #endif
    |> Program.withReactBatched appElementId
    #if DEBUG
    |> Program.withDebugger
    #endif
    |> Program.runWith (Router.currentUrl())


App "root"
