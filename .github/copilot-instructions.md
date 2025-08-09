# .github/copilot-instructions.md

## Mod Overview and Purpose

**Mod Name:** Save modlist as collection

The "Save modlist as collection" mod for RimWorld allows players to easily save their current in-game mod list as a Steam Workshop collection. This feature is particularly useful for organizing mods, sharing them with other players, or preserving a setup for future playthroughs. The mod streamlines the process of creating and updating collections by integrating directly with the game's mod list interface.

## Key Features and Systems

- **Collection Management:** Easily save the current mod list as a new Steam Workshop collection.
- **Update Existing Collections:** Players can update their existing collections with the current mod list, ensuring their collections are up to date.
- **User Interface Integration:** The mod provides user-friendly dialogs to facilitate collection creation and updates directly within the game.

## Coding Patterns and Conventions

- **Name Conventions:**
  - Classes use PascalCase.
  - Methods use PascalCase.
  - Use clear, descriptive names for methods and variables to improve readability.

- **Organization:**
  - Group related methods logically to facilitate understanding and maintenance.
  - When possible, create helper classes or methods to prevent code duplication.

- **Error Handling:**
  - Implement appropriate try-catch blocks to handle potential exceptions especially when interacting with Steam's API.

## XML Integration

- Although this mod is primarily coded in C#, XML integration can be used for RimWorld's mod settings or to define new dialog windows in some cases. Familiarity with RimWorld's XML schema will be beneficial for designing UI elements and integrating with the game's settings.

## Harmony Patching

- **Purpose and Use:**
  - Harmony is a library for patching, replacing, and altering compiled code at runtime. This mod might use Harmony to ensure the mod list can be intercepted, modified, or extended.
  
- **Conventions:**
  - Ensure that patches are well-defined and scoped to avoid unintended side effects.
  - Prefix harmony patches with "Patch_" followed by the original method name for clarity.
  
- **Examples:**
  - Override or extend existing functionality, such as the method that displays the mod list in RimWorld.
  - Use annotations like `[HarmonyPatch(typeof(TargetClass), "TargetMethod")]` to specify the target and method to be patched.

## Suggestions for Copilot

- **Contextual Suggestions:**
  - Focus on offering assistance when common patterns are emerging, such as dialog window management and update loops.
  
- **Method Stubs and Auto-Completion:**
  - For classes like `CollectionUpdateStatus_Window` and `Dialog_ModCollections`, suggest method stubs related to user interface actions and event handling.
  
- **Code Comments:**
  - Encourage adding comments, especially around critical methods handling Steam API calls, to improve code understanding and maintenance.
  
- **Harmony Integration:**
  - Provide inline suggestions for common patching patterns and encourage the use of Harmony best practices.
  
- **Error Handling:**
  - Suggest patterns for handling potential exceptions when interacting with APIs, especially around network requests and file I/O. 

By following these guidelines, developers can effectively use GitHub Copilot to assist with the development and maintenance of the "Save modlist as collection" mod, ensuring a smooth development experience and a high-quality output.
