# AGSModuleExporter: Script Module Exporter for Adventure Game Studio

by monkey0506

This tool is designed to assist in automated build tasks that require an AGS script module SCM file to be exported from the ASC script and ASH header files.

## Running the tool

This is a command line program. On Windows, you can simply call "AGSModuleExporter.exe" with the necessary parameters. The tool may also be run under Mono for other OSes.

### Command line arguments

- *-script <script file>*

  **REQUIRED.** The location of the script .ASC or .ASH file. Missing script/header is defaulted to empty text. As the script and header files vary only by their extension, you may also give just the filename without an extension and the tool will load the script and header with that filename.

- *-module <exported module file>*

  *Optional.* The filename of the exported script module. ".scm" file extension is added if not present. If this argument is not specified, it defaults to the same filename as the script and header files.

- *-help*

  Prints help text with argument descriptions.

### Setting the module name, description, author, version, and UniqueKey

You may *optionally* supply an XML file with the same filename as the script and header file to set the module's metadata. There is not a strict format for the file, but the XML nodes used match those used in Game.agf, which must be children of the root element.

    <?xml version="1.0"?>
    <AGSModule>
      <Name>NAME</Name>                      <!-- set the module name -->
      <Description>DESCRIPTION</Description> <!-- set the module description -->
      <Author>AUTHOR</Author>                <!-- set the module author -->
      <Version>VERSION</Version>             <!-- set the module version -->
      <Key>UNIQUE_KEY</Key>                  <!-- set the module unique key -->
    </AGSModule>

Any of these values may be omitted. All of the values except the unique key are text (version may include "alpha" or other tags, for example).

The unique key is an integer and is not a user-visible property, but is used to ensure that the same script module isn't imported into the same project several times. Therefore, if your module has previously been distributed then you should get its unique key from the Game.agf XML file. Otherwise, a unique key will be generated for you, but you should then be sure to save it for future use.

### Example

    AGSModuleExporter.exe -script "C:\User\monkey0506\My Documents\AGS\Modules\Stack\Stack.asc" -module "C:\User\monkey0506\My Documents\AGS\Modules\Stack.scm"
