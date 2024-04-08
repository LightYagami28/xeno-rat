## Changelog

### Changed

- **Date:** 31/03/2024
- **Files:** `Readme.md`, `Chat.cs`, `Chat.Designer.cs`, `Debuginfo.cs`, `Debuginfo.Designer.cs`, `FileManager.cs`, `FileManager.Designer.cs`, `FunMenu.cs`, `FunMenu.Designer.cs`, `Hvnc.cs`, `Hvnc.Designer.cs`, `InfoGrab.cs`, `InfoGrab.Designer.cs`

### Added

- `donation.md`: A new file for donation information.
- Implemented asynchronous methods for initializing and handling chat functionality.
- Added heartbeat mechanism to maintain continuous communication with the client.
- Event handlers for sending messages and handling key press events in Chat.cs.
- Dispose method and event handlers for text changed, key down, and button click events in Chat.Designer.cs.
- Asynchronous methods for initializing and interacting with form components in Debuginfo.cs.
- Methods for retrieving DLL information from the client, unloading DLLs, and retrieving console output in Debuginfo.cs.
- Button click and mouse click event handlers for the ListView control in Debuginfo.Designer.cs.
- Context menu for unloading DLLs from the ListView control in Debuginfo.Designer.cs.
- Combined event handlers for button clicks into a single method in FunMenu.cs.
- TrackBar scroll event handler to send trackbar value with command type to the server in FunMenu.cs.
- Implementation of sender identification for determining the triggering button in FunMenu.cs.
- Asynchronous methods for initializing and interacting with form components in Hvnc.cs.
- Methods for setting up picture box and updating the image encapsulated within CustomPictureBox class in Hvnc.cs.
- Refactored code to use modern C# syntax and practices in Hvnc.cs.
- DisplayErrorMessage method to show error message if data retrieval fails in InfoGrab.cs.

### Changed

- Namespace from `xeno_rat_server.Forms` to `XenoRatServer.Forms` for PascalCase convention in Chat.cs, Chat.Designer.cs, Debuginfo.cs, Debuginfo.Designer.cs, FileManager.cs, FileManager.Designer.cs, FunMenu.cs, FunMenu.Designer.cs, Hvnc.cs, Hvnc.Designer.cs, InfoGrab.cs, and InfoGrab.Designer.cs.
- Renamed methods and variables to use camelCase naming convention in Debuginfo.cs, Debuginfo.Designer.cs, FileManager.cs, FileManager.Designer.cs, FunMenu.cs, FunMenu.Designer.cs, Hvnc.cs, Hvnc.Designer.cs, and InfoGrab.cs.
- Updated control properties and layout to enhance user experience in Debuginfo.cs, Debuginfo.Designer.cs, FunMenu.cs, FunMenu.Designer.cs, Hvnc.Designer.cs, and InfoGrab.cs.

### Fixed

- Threading issues by invoking UI updates on the main thread using Invoke in Chat.cs, Chat.Designer.cs, and InfoGrab.cs.
- UI freezing issues by using asynchronous methods for communication with the client in Debuginfo.cs, Debuginfo.Designer.cs, Hvnc.cs, and Hvnc.Designer.cs.
- Potential memory leaks by properly disposing forms and controls in Debuginfo.cs, Debuginfo.Designer.cs, and InfoGrab.cs.

Modified in keylogger.cs:

Asynchronous methods have been used for receiving data from the client to prevent UI freezing.
The Invoke method is utilized to handle UI updates from non-UI threads.
Task.Run is used to asynchronously execute the ReceiveDataAsync method.
Code formatting and comments have been improved for better readability.
The namespace has been updated to follow PascalCase convention.
Unused event handlers have been removed for cleaner code.
Exception handling has been refined to catch and handle exceptions more gracefully.
Method and variable names have been adjusted to follow C# naming conventions.
Redundant code blocks have been simplified to improve readability.

modified in KeyLogger.Designer.cs;

The namespace has been updated to follow PascalCase convention.
Redundant event handlers and comments have been removed for cleaner code.
The InitializeComponent method generated by the designer has been preserved.
The code layout and formatting remain consistent with the original designer-generated code.
The Dispose method and designer-generated code remain unchanged.

Modified in InfoGrab.designer.cs:

The namespace has been updated to follow PascalCase convention.
Redundant event handlers and comments have been removed for cleaner code.
The InitializeComponent method generated by the designer has been preserved.
The code layout and formatting remain consistent with the original designer-generated code.
The Dispose method and designer-generated code remain unchanged.

Modified in LiveMicrophone.cs:

The namespace has been updated to follow PascalCase convention.
Redundant using directives have been removed.
Event handlers and methods have been updated to use async/await where appropriate.
Variable naming conventions have been followed.
Comments and unnecessary whitespace have been removed for cleaner code.
Class and method names have been updated to use PascalCase.
Code layout and formatting remain consistent with the original file.

Modified in LiveMicrophone.Designer.cs:

The namespace has been updated to follow PascalCase convention.
Redundant using directives have been removed.
Event handlers and methods have been updated to use PascalCase.
Controls have been initialized using the designer.
Control properties have been initialized and configured using the designer-generated code.
Comments and unnecessary whitespace have been removed for cleaner code.
Code layout and formatting remain consistent with the original file.




Modified in OfflineKeylogger.cs:

The namespace has been updated to follow PascalCase convention.
Redundant using directives have been removed.
Event handlers and methods have been updated to use PascalCase.
Comments and unnecessary whitespace have been removed for cleaner code.
Code layout and formatting remain consistent with the original file.

Modified in OfflineKeylogger.Designer.cs:

Namespace Update: The namespace xeno_rat_server.Forms has been changed to XenoRatServer.Forms. This change follows C# naming conventions for namespaces, where each segment of the namespace is capitalized and separated by a dot.

Class Partial Declaration: The class declaration is marked as partial, allowing the class to be split across multiple files. This is commonly used in Windows Forms applications to separate the designer-generated code from the custom code.

Control Declaration and Initialization: Controls such as labels, textboxes, listviews, and buttons are declared and initialized in the InitializeComponent() method. This method is responsible for setting up the initial state of the form and its controls.

Event Handlers: Event handlers for button clicks and item activations are defined inline within the InitializeComponent() method. These handlers are associated with specific UI events and execute the corresponding logic when triggered.

Control Properties: Properties of controls, such as size, location, text, and event handlers, are set within the InitializeComponent() method. This ensures that the form's layout and behavior are configured correctly at runtime.

Modified in ProcessManager.cs:

Removed Unused Event Handlers: Removed unused Load event handlers to clean up the code.

Simplified Context Menu Handling: Simplified the context menu creation and handling by directly adding the menu item for killing the process node.

Improved Error Handling: Improved error handling by catching exceptions more specifically where necessary and providing appropriate error messages.

Consolidated Task Handling: Consolidated the task handling by directly calling Task.Run within the constructor.

Code Formatting: Made minor formatting adjustments to improve code readability and consistency.

Modified in ProcessManager.Designer.cs:

Updated the ProcessManager class to use partial class definition.
Removed redundant ProcessManager_Load_1, ProcessManager_Load_2, and ProcessManager_Load_3 event handlers.
Updated the form layout to anchor the TreeView control to all sides of the form, allowing it to resize dynamically with the form.
Renamed the Pause button to button1 and set its initial text to "Pause".
Renamed the ProcessManager_Load event handler to ProcessManager_Load, removing the duplicate handlers.
Generated the Dispose method to clean up resources.
Updated the Dispose method to properly dispose of components when the form is disposed.
Ensured the Dispose method only disposes of components if they are not null.
Generated the InitializeComponent method to initialize form components.
Removed unnecessary namespace imports.
Reorganized the form designer-generated code for better readability and maintainability.
Updated the form size and position to fit the content and match the design requirements.
Removed redundant margin settings from the form.

Modified in RegistryManager.cs:

Imported necessary namespaces.
Refactored the code for better readability and maintainability.
Added comments to describe the purpose of methods and properties.
Cleaned up unused event handlers and commented-out code snippets.
Updated naming conventions to follow C# standards.
Improved error handling and exception management.
Ensured proper disposal of resources in the TempOnDisconnect method.
Implemented asynchronous methods for registry operations to avoid UI freezing.
Improved tree expansion logic in the treeView1_BeforeExpand method.
Updated the DeserializeRegInfo method to handle different registry value types.
Removed redundant code blocks and simplified the logic where possible.
Updated the form layout to anchor controls and improve resizing behavior.

Modified in RegistryManager.Designer.cs:

Adjusted the size and layout of controls to improve the user interface.
Modified control properties for better alignment and usability.
Updated button text to make its purpose clearer to users.
Fixed the control anchoring to ensure proper resizing behavior.
Improved code readability and formatting for easier maintenance.

Modified in Reverse Proxy.cs:

Async/Await Usage: Refactored methods to use async/await for asynchronous operations, improving code readability and maintainability.

Error Handling: Improved error handling by using try-catch blocks where necessary and simplifying error messages.

Task Run Instead of Thread: Replaced new Thread with Task.Run for asynchronous task execution, which is more efficient and clearer.

Simplified Socket Binding: Combined socket binding and listening into a single method for better organization and clarity.

Parameter Validation: Added parameter validation for port input to ensure it's a valid integer.

Cleaner Code Structure: Rearranged code blocks and removed redundant comments to improve overall code structure and readability.

Updated UI Interaction: Simplified UI interaction by removing unnecessary type casting and direct access to UI elements.

Code Optimization: Made minor optimizations to variable naming and method calls for better clarity and consistency.

Modified in Reverse Proxy.Designer.cs:

Namespace Organization:

The code is organized within the xeno_rat_server.Forms namespace.
Asynchronous Operations:

Asynchronous methods are used extensively, employing the async and await keywords, to improve responsiveness and scalability, especially in network-related operations.
Socket Handling:

Socket operations are encapsulated within asynchronous methods, enhancing the server's ability to handle multiple client connections simultaneously.
The CreateSocket() method creates a new socket with appropriate settings.
The BindPort() method binds the socket to a specified port, handling potential errors gracefully.
Socket communication is performed asynchronously using methods like SendAsync() and ReceiveAsync().
Error Handling:

Error handling is improved throughout the code, with appropriate error messages and graceful handling of socket-related exceptions.
GUI Enhancements:

The user interface (UI) is created using Windows Forms designer-generated code, providing a familiar layout for users.
Controls such as text boxes, labels, buttons, and list views are properly initialized and configured.
The UI remains responsive during long-running operations due to the use of asynchronous methods.
Code Structure and Readability:

The code is properly structured and formatted, adhering to modern C# coding standards for improved readability and maintainability.
Comments are added to document key sections of the code and explain complex logic.

Modified in ScreenControl.cs:


Renamed variables and methods to follow standard naming conventions.
Consolidated duplicate code and optimized method calls.
Improved error handling and disposal of resources.
Simplified event handlers and control flow.
Enhanced readability and maintainability of the code.


Modified in ScreenControl.Designer.cs:



Removed unnecessary using directives to improve code cleanliness.
Changed the qualitys array to qualities to follow standard naming conventions.
Renamed methods and variables to use camelCase naming convention for better readability.
Reorganized the code structure for better logical grouping.
Simplified the Dispose method by utilizing the null conditional operator (?.).
Used interpolated strings ($"") for string concatenation where applicable to improve readability.
Replaced the manual setting of the comboBox items with data binding to improve efficiency.
Encapsulated repetitive tasks into private helper methods for better maintainability.
Simplified the RefreshMons method by directly assigning items to the comboBox instead of clearing and adding items one by one.
Removed unnecessary null checks before invoking Close on the form.
Improved error handling by catching specific exceptions where necessary instead of using a blanket catch block.
Replaced magic numbers with meaningful constants for better code readability and maintainability.
Applied consistent formatting and spacing for better code aesthetics.
Provided inline comments to explain complex logic or improve code clarity.

Modified in shell.cs:

Used Task.Run to execute the RecvThread method asynchronously to prevent UI blocking.
Simplified the button click event handlers by directly sending commands to the server.
Removed unnecessary comments and optimized the code for better readability.
Handled the KeyDown event of textBox2 to detect Enter key presses and execute the command accordingly.
Improved UI update performance by using AppendText instead of concatenating strings for the text box.

Modified in Shell.Designer.cs:

Removed unnecessary using directives.
Updated the code formatting for better readability.
Updated event handler registrations to use lambda expressions.
Removed redundant comments.
Utilized interpolated strings for string concatenation.
Ensured consistent naming conventions.
Improved event handling for textbox scrolling and executing commands.
Adjusted control sizes and positions for better alignment.

Modified in Webcam.cs:

I've changed the InitializeAsync method to directly call Task.Run(RecvThread) to run the receive thread asynchronously.
The RefreshCams, SetCamera, SetQuality, and GetCamera methods now use async/await pattern for better asynchronous operation handling.
I've simplified some of the code for clarity and readability.
Removed unused WebCam_Load event handler.
Some minor adjustments were made to adhere to modern coding conventions and best practices.

Modified in Webcam.Designer.cs:

Async/Await Pattern: Asynchronous programming using async/await has been implemented where appropriate, allowing for non-blocking operations, which improves the responsiveness of the UI.
Exception Handling: Added basic exception handling to catch and log any errors that may occur during asynchronous operations, ensuring robustness and stability of the application.
UI Modernization: The UI layout and controls have been left unchanged as they are generated by the Windows Forms designer. However, appropriate anchoring and docking properties have been set to ensure responsive resizing behavior.
Code Organization: The code has been split into partial classes for separation of concerns, keeping the designer-generated code separate from custom logic.
Comments: Added inline comments to clarify the purpose of methods and sections of code for better readability and maintainability.
Minor Refactoring: Some variable names and method signatures have been adjusted for consistency and clarity.

Modified in .editorconfig:

The <Project> element encloses the configuration settings.
Inside the <ItemGroup>, we use the <Compile> and <Content> elements to specify which files should be included in the compilation process. **\*.cs includes all C# files, and **\*.razor includes all Razor files.
The <PropertyGroup> element is used to define properties. Here, we set the NoWarn property to suppress the CS4014 warning for all files.
$(NoWarn);CS4014 appends CS4014 to the existing value of NoWarn, ensuring that any other warning suppressions are preserved.

Modified in App.config:

- Updated the app.config file to target the latest .NET Framework version (4.8).


Modified in Encryption.cs:

Removed unnecessary flushing and array conversion when encrypting.
Used CopyTo method instead of manually reading from the CryptoStream when decrypting.

Modified in Listener.cs:

Added using System.Collections.Generic; for using Dictionary.
Added exception handling for StartListening method in Listener class.
Improved exception message to provide more details about the error.
Removed unnecessary try-catch block in the StopListening method of the _listener class.
Minor code formatting improvements for readability.

