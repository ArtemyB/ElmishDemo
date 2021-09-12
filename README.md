# Fable + Elmish + Feliz + Fable.Form + Material UI demo app

This app is created to demonstrate the issue of excessive re-rendering. The problem is that the whole global Elmish-state dependent part of the app tends to re-render on every app state change (according to what React Dev Tools shows with "Highlight updates when components render" setting being enabled), even though only necessary parts of the global state are passed to child views. For example Login form's state is a part of the global Elmish state, and even one form's input change causes the whole app UI to re-render (even AppBar, which is independent from Login form values).

The demo app consists of 2 pages: Login and Home. Login page is the default one, it requires from user to enter user's name (any nonempty string) and a password (also any non-empty string). After that the app redirects the user to the Home page, which will only output current user's name. So the Home page is not available untill Log in form is submitted.

## Requirements

* [dotnet SDK](https://www.microsoft.com/net/download/core) 5.0 or higher
* [node.js](https://nodejs.org)
* An F# editor like Visual Studio, Visual Studio Code with [Ionide](http://ionide.io/) or [JetBrains Rider](https://www.jetbrains.com/rider/)

## Building and running the app

* Install dependencies: `npm install`
* Start the compiler in watch mode and a development server: `npm start`
* After the first compilation is finished, in your browser open: http://localhost:8080/

Any modification you do to the F# code will be reflected in the web page after saving.

> Note: check the "scripts" section in `package.json` to see the commands triggered by the steps above.

## Bundling for release

Run the following command to compile and bundle up all your F# code into one Javascript file: `npm run build`. The compiled output ends up in the `public` folder under the name `bundle.js`.

## Project structure

### npm

JS dependencies are declared in `package.json`, while `package-lock.json` is a lock file automatically generated.

### Webpack

[Webpack](https://webpack.js.org) is a JS bundler with extensions, like a static dev server that enables hot reloading on code changes. Configuration for Webpack is defined in the `webpack.config.js` file. Note this sample only includes basic Webpack configuration for development mode, if you want to see a more comprehensive configuration check the [Fable webpack-config-template](https://github.com/fable-compiler/webpack-config-template/blob/master/webpack.config.js).

### F#

The sample only contains two F# files: the project (.fsproj) and a source file (.fs) in the `src` folder.

### Web assets

The `index.html` file and other assets like an icon can be found in the `public` folder.
