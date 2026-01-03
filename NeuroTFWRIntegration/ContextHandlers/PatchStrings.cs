namespace NeuroTFWRIntegration.ContextHandlers;

public static class PatchStrings
{
	public const string SearchParser = """
	                                   Where [YOUR_DIFF] consists of one or more file-scoped edit blocks. Each block begins with the path to the file to be edited, followed by a fenced diff block containing one sections.

	                                   The syntax for each file edit block is:

	                                   [path/to/file]
	                                   ```
	                                   <<<<<<< SEARCH
	                                   [exact text to search for]
	                                   =======
	                                   [replacement text]
	                                   >>>>>>> REPLACE
	                                   ```

	                                   ### Rules for diff blocks

	                                   1. **Exact Matching**
	                                      - The text inside `<<<<<<< SEARCH` and `=======` must match the existing file content verbatim.
	                                      - The system performs literal substring replacement, so spacing, indentation, and newlines must match exactly.

	                                   2. **Replacement Text**
	                                      - The text between `=======` and `>>>>>>> REPLACE` becomes the new code that replaces the search block.
	                                      
	                                   3. **Multiple Replacements**
	                                    - A single file block may include multiple search/replace pairs, one after another, each using its own `<<<<<<< SEARCH … ======= … >>>>>>> REPLACE` section.
	                                    - Edits are processed in the order they appear.
	                                    - You should never make a pair that is based on a section that is being added. If you are searching for text that is also being added in that patch, your patch will be rejected. You should try to implement as much as possible in a singular section.
	                                    
	                                   4. **Multiple Files**
	                                    - You may include as many file blocks as needed in a single diff command. Each must begin with the file path on its own line, immediately followed by its fenced diff block.
	                                    - You should try to only have 1 line of whitespace between each file block, more will still work though.
	                                   
	                                   3. **No Context Requirement**
	                                      - Unlike traditional patch formats, this diff format does *not* require context lines or line numbers. Only the exact search/replace texts are used.

	                                   4. **Adding or Removing Code**
	                                      - If a file already contains code and the search is empty, the whole file will be replaced with the replace section so be careful
	                                      - To insert new code, use an empty SEARCH section:
	                                        ```
	                                        <<<<<<< SEARCH
	                                        =======
	                                        [your new code]
	                                        >>>>>>> REPLACE
	                                        ```
	                                      - To delete code, leave the REPLACE section empty:
	                                        ```
	                                        <<<<<<< SEARCH
	                                        [code to delete]
	                                        =======
	                                        >>>>>>> REPLACE
	                                        ```

	                                   ### Example
	                                   - This is what an example of what you should send in the json string

	                                   main
	                                   ```
	                                   <<<<<<< SEARCH
	                                   harvest()
	                                   return 19
	                                   =======
	                                   move(North)
	                                   harvest()
	                                   print("Neuro is very stinky, because she smells like a gymbag.")
	                                   return 21
	                                   >>>>>>> REPLACE
	                                   ```

	                                   """;
}