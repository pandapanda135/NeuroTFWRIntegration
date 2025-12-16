namespace NeuroTFWRIntegration;

public static class PatchStrings
{
	public const string SearchParser = """
	                                   %%bash
	                                   diff <<"EOF"
	                                   [YOUR_DIFF]
	                                   EOF

	                                   Where [YOUR_DIFF] consists of one or more file-scoped edit blocks. Each block begins with the path to the file to be edited, followed by a fenced diff block containing one or more search/replace sections.

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

	                                   4. **Multiple Files**
	                                      - You may include as many file blocks as needed in a single diff command. Each must begin with the file path on its own line, immediately followed by its fenced diff block.

	                                   5. **No Context Requirement**
	                                      - Unlike traditional patch formats, this diff format does *not* require context lines or line numbers. Only the exact search/replace texts are used.

	                                   6. **Adding or Removing Code**
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

	                                   mathweb/flask/app.py
	                                   ```
	                                   <<<<<<< SEARCH
	                                   from flask import Flask
	                                   =======
	                                   import math
	                                   from flask import Flask
	                                   >>>>>>> REPLACE
	                                   ```

	                                   """;

	// this is the JSON compliant string you can test with.
	public const string TestingString =
		"main\n```\n<<<<<<< SEARCH\nfrom variable_values import max_pos\nwhile True:\n\tmove(North)\n\tif can_harvest():\n\t\tharvest()\n\tif get_pos_y() == max_pos:\n\t\tmove(East)\n=======\nfrom variable_values import max_pos\nwhile True:\n\tmove(North)\n\tif can_harvest():\n\t\tharvest()\n\tif get_pos_y() == max_pos:\n\t\tmove(East)\n\t\twhile get_pos_y() > 0:\n\t\t\tmove(South)\n>>>>>>> REPLACE\n```";
}