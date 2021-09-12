module Fable.Form.MaterialUI

open System
open Fable.React
open Feliz
open Fable.Form.Simple
open Feliz.MaterialUI


let inline textFieldErrorOutput (config: Form.View.TextFieldConfig<'Msg>) =
    match config.ShowError, config.Error with
    | true, Some err ->
        Mui.formHelperText [
            formHelperText.error true
            formHelperText.children (
                match err with
                | Error.RequiredFieldIsEmpty ->
                    "Field is required"
                | Error.External msg -> msg
                | Error.ValidationFailed msg -> msg
            )
        ]

    | false, Some _
    | _, None -> Html.none

[<ReactComponent>]
let FormTextField
    (config: Form.View.TextFieldConfig<'Msg>)
    =
    let inputId = Guid.NewGuid().ToString()
    Mui.formControl [
        formControl.fullWidth true
        formControl.size.medium
        formControl.margin.normal
        formControl.error (config.ShowError && config.Error.IsSome)

        formControl.children [
            Mui.inputLabel [
                prop.htmlFor inputId
                inputLabel.children config.Attributes.Label
            ]
            Mui.input [
                input.id inputId
                prop.type'.text
                input.autoFocus true
                input.onBlur (fun e -> config.OnBlur |> Option.iter config.Dispatch)
                input.onChange (fun (v: string) -> config.OnChange v |> config.Dispatch)
                input.disabled config.Disabled
                prop.value config.Value
                input.fullWidth true
            ]
            textFieldErrorOutput config
        ]
    ]


[<ReactComponent>]
let PasswordInput
    (config: Form.View.TextFieldConfig<'Msg>)
    =
    let showPassword, setShowPassword = React.useState false
    let inputId = Guid.NewGuid().ToString()

    Mui.formControl [
        formControl.fullWidth true
        formControl.size.medium
        formControl.margin.normal
        formControl.error (config.ShowError && config.Error.IsSome)

        formControl.children [
            Mui.inputLabel [
                prop.htmlFor inputId
                inputLabel.children config.Attributes.Label
            ]
            Mui.input [
                input.id inputId
                input.type' (if showPassword then "text" else "password")
                input.onChange (fun v -> config.OnChange v |> config.Dispatch)
                prop.value config.Value
                input.autoComplete "current-password"
                input.onBlur (fun e -> config.OnBlur |> Option.iter config.Dispatch)
                input.fullWidth true
                input.endAdornment (
                    Mui.inputAdornment [
                        inputAdornment.position.end'
                        inputAdornment.children(
                            Mui.iconButton [
                                prop.onMouseDown (fun e -> e.preventDefault())
                                prop.onClick (fun e -> setShowPassword (not showPassword))
                                prop.children (
                                    if showPassword then
                                        Fable.MaterialUI.Icons.visibilityIcon []
                                    else
                                        Fable.MaterialUI.Icons.visibilityOffIcon []
                                )
                            ]
                        )
                    ]
                )
            ]
            textFieldErrorOutput config
        ]
    ]


let inline materialUIFormConfig<'Msg>
    (formGridProps)
    (submitBtnProps)
    (resetMsg: 'Msg option)
    //(formSubmit: Form.View.State -> Elmish.Dispatch<'Msg> -> 'Msg option -> ReactElement)
    : Form.View.CustomConfig<'Msg> =
    {
        Form = fun f ->
            Html.form [
                prop.disabled (f.State = Form.View.State.Loading)
                prop.onSubmit (fun ev ->
                    ev.stopPropagation()
                    ev.preventDefault()

                    f.OnSubmit
                    |> Option.map f.Dispatch
                    |> Option.defaultWith ignore
                )

                prop.children [
                    Mui.grid [
                        grid.container true
                    
                        yield! formGridProps

                        grid.children [
                            for field in f.Fields do
                                Mui.grid [
                                    grid.item true
                                    grid.children field
                                ]

                            //formSubmit f.State f.Dispatch f.OnSubmit
                            Mui.grid [
                                grid.item true
                                grid.children [
                                    match f.State with
                                    | Form.View.State.Error _
                                    | Form.View.State.Idle ->
                                        Mui.button [
                                            prop.type'.submit
                                            button.color.primary

                                            if f.OnSubmit.IsNone then
                                                prop.disabled true

                                            yield! submitBtnProps
                                            //prop.onClick (fun _ ->
                                            //    match f.OnSubmit with
                                            //    | Some msg ->
                                            //        msg
                                            //        |> f.Dispatch
                                            //    | None -> ())
                                            button.children f.Action
                                        ]

                                    | Form.View.Loading ->
                                        Mui.circularProgress [
                                            circularProgress.variant.indeterminate
                                        ]
                                    | _ -> Html.none
                                ]
                            ]

                            match f.State with
                            | Form.View.State.Error errMsg ->
                                Mui.grid [
                                    grid.item true
                                    grid.children [
                                        Mui.alert [
                                            alert.severity.error
                                            alert.color.error
                                            match resetMsg with
                                            | Some resetMsg ->
                                                alert.action [
                                                    Mui.button [
                                                        button.color.inherit'
                                                        button.size.small
                                                        prop.onClick (fun _ -> f.Dispatch resetMsg)
                                                        button.children "Сброс"
                                                    ]
                                                ]
                                            | None -> yield! []
                                            alert.children errMsg
                                        ]
                                    ]
                                ]

                            | Form.View.State.Success successMsg ->
                                Mui.grid [
                                    grid.item true
                                    grid.children [
                                        Mui.alert [
                                            alert.severity.success
                                            alert.color.success
                                            alert.children successMsg
                                        ]
                                    ]
                                ]

                            | Form.View.State.Idle
                            | Form.View.State.Loading ->
                                Html.none
                        ]
                    ]
                ]
            ]
        TextField = FormTextField
        PasswordField = PasswordInput
        EmailField = fun _ -> Html.none
        TextAreaField = fun _ -> Html.none
        CheckboxField = fun _ -> Html.none
        RadioField = fun _ -> Html.none
        SelectField =fun _ -> Html.none
        Group = fun _ -> Html.none
        Section = fun title fields -> Html.none
        FormList = fun _ -> Html.none
        FormListItem = fun _ -> Html.none
    }


let renderMaterialUIForm (formGridProps) (submitBtnProps) (resetMsg) (config) =
    Form.View.custom (materialUIFormConfig formGridProps submitBtnProps resetMsg) config

