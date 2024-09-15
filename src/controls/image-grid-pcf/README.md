# Image Grid PCF
This Power Apps Component Framework Control that shows a slider of a images bound to a dataset data source.

## Building

To build the PCF component using:

```shell
npm run build
```

To start the build in watch mode use:

```shell
npm start watch
```


## Debugging

To debug the code component after it is deployed to Power Apps you can use [Fiddler Classic](https://www.telerik.com/fiddler/fiddler-classic) with an **Auto Responder** rule.

1. Download and install [Fiddler Classic](https://www.telerik.com/fiddler/fiddler-classic).

1. Open **Fiddler** and from the menu bar, go to **Tools**, and then select **Options**.

1. Select the **HTTPS** tab in the dialog box and check the Capture HTTPS CONNECTS and **Decrypt HTTPS traffic** checkboxes so that the HTTPS traffic is captured and then decrypted.

1. Select the marked checkboxes in the **HTTP** tab.

1. Select **OK** to close the dialog box.

    > [!NOTE]
    > If it is the first time you are enabling this setting, Fiddler will prompt you to install a certificate. Install the certificate and restart Fiddler so that the new settings take effect.
    If you have run Fiddler in the past and get a NET::ERR_CERT_AUTHORITY_INVALID error, in the HTTPS tab, select the Actions button and choose Reset All Certificates. This will also present multiple prompts for the new certificates to be installed.

1. In the right-hand panel, select the **AutoResponder** tab.

1. Ensure that **Enable Rules** and Unmatched requests passthrough are checked.

1. To add the auto responder for Canvas Apps, select Add Rule and enter first:

    ```text
    regex:(?inx).+Resources0Controls0Contoso.ImageGrid.bundle.js
    ```

1. Add the location of your bundle.js, e.g. 
    ```text
    C:\repos\contoso-real-estate-power\src\controls\image-grid-pcf\out\controls\ImageGrid\bundle.js
    ```

1. Repeat for Model driven apps using the expression:

    ```text
    regex:(?inx).+ImageGrid/bundle.js
    ```

1. Select **Save**.

1. Select the **FiddlerScript** tab.

1. Locate the function `static function OnBeforeResponse(oSession: Session)`

1. Add the following inside the function:

    ```VBScript
    if (oSession.oFlags.ContainsKey("x-repliedwithfile")) {
        oSession.oResponse["x-fiddler"] = oSession.oFlags["x-repliedwithfile"];
        // Set the Access-Control-Allow-Origin header to *
        oSession.oResponse["Access-Control-Allow-Origin"] = "*";
    }
    ```

    This will ensure that the auto response will accepted by the CORS (Cross-Origin Resource Sharing) checks.

1. Now that you have the AutoResponder rules running you will need to first clear the cache in the browser and reload the page containing the code component. This can easily be done by opening developer tools (`Ctrl + Shift + I`), right-clicking the **Refresh** > **Empty cache and hard refresh**.

1. Once you have your code component loaded from your local machine, you can make changes to the code (while `npm start watch` is running) and refresh the browser to load the newly built versions. Fiddler's AutoResponder will automatically add a `cache-control` header so that it will not be cached by the browser so a simple refresh will reload the resources without having to clear the cache each time.