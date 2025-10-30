<h1> GUID Reconnector </h1>

This tool was designed to help move assets between Unity projects that may ultimately share resources but may have different GUIDs between projects. This tool was started as a personal project to help with [Lethal Company Modding](https://discord.gg/XeyYqRdRGC) but has been expanded to be less hard-coded/cumbersome and more general usage. However, it should also work in other use cases as well. A video tutorial is linked at the bottom of this page.

<h2>Importing</h2>

 1. Copy the URL provided when you click the green `<> Code` button at the top of the page.
 2. Inside your Unity project(s) up at the top toolbar click through `Window`>`Package Manager`.
 3. With the Packager Manager window up, click the `+` icon top left > `Install package from git URL...`.
 4. Paste in the URL and click `Install`.

<h2>Usage</h2>

Once imported you should be able to go to the top toolbar and click through `Tools`>`GUID Reconnector`. This should provide you with two options, `Extract Info` and `Import Info`. In the project you are going to be moving things from, you will run `Extract Info` and eventually in the project you are moving your things into you will run `Import Info`.

![Image showing options](https://imgur.com/GiPOdsy.png)

<h3>Extract Info</h3>

In the project you wish to extract references from, you are prompted to select two folders. <br>
`Folder to move`: The folder that you intend to copy into your other project (example: a folder of prefabs). <br>
`Base folder`: The folder containing the other stuff that your assets should reconnect to (example: meshes, materials, audio clips, scripts, etc.). <br>
Select these directories and then click the `Extract and Save JSON` button, chose where to save the file, and wait for it to complete. A log will also be produced at this location. 

![Extract UI](https://imgur.com/75g12Cx.png)

<h3>Import Info</h3>

In the folder you are moving your stuff into, you are prompted to select two folders. <br>
`Folder to move`: The folder that you have dragged into your Unity project and need to reconnect its contents (example: a folder of prefabs). <br>
`Base folder`: The folder containing the other stuff that your assets should reconnect to (example: meshes, materials, audio clips, scripts, etc.). <br>
Select these directories and then click the `Import and Reconnect Connections Using Saved JSON` button, chose the previously created JSON file, and wait for it to complete. A log will also be produced at the JSON's original location.

![Import UI](https://imgur.com/pFycOek.png)

<h2>Disclaimers</h2>

1. Make backups of your project. Extracting info should not make any changes but importing potentially could change unintended GUID references if the selected `Folder to move` contains more than just what was brought over.
2. This tool may not 100% reconnect everything. I've noticed terrains that reference terrain layer assets do not reconnect and inside scripts that reference prefabs sometimes the prefab is not reconnected.
3. Assets that reference different assets that happen to share a name could be incorrectly relinked.

Ultimately, backup your projects and check that the import process properly reconnected all of the intended references! Depending on how your poject is set up you should be able to do multiple extracts/imports just fine if you need to.

<h2>Other Useful Links</h2>

[YouTube tutorial/example](https://youtu.be/oEfzqSVsuaU) <br>
[NG Missing Script Recovery](https://assetstore.unity.com/packages/tools/utilities/ng-missing-script-recovery-102272) for help relinking scripts if this tool doesn't reconnect them.