# Development Setup

For the development on the RelayServer the following steps have to be carried out:

* Installing Node.js with a version specified in the `.nvmrc` file (NVM for Windows can not currently retrieve`.nvmrc` files).
* Install the necessary Node packages: `npm install`.
* Run Gulp once with the task `npm run watch`
  This creates the .css files from the .less files.
* In the `App.config` file, set the `ManagementWebLocation` setting to the value `"..\..\..\Thinktecture.Relay.ManagementWeb"` so that the RelayServer can find and deliver the ManagementWeb.
  Alternatively, the `\Thinktecture.Relay.ManagementWeb` folder can also be copied to the `\Thinktecture.Relay.Server\bin\{Configuration}\` folder.
