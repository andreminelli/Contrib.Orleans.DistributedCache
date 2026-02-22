## C# Coding Standards

*   **Version**: Use .Net 10 and C# 14 features 
*   **Braces:** Always include braces (`{}`) for all control statements (e.g., `if`, `for`, `while`), even for single-line statements, to improve readability and prevent errors.
*   **Indentation:** Use 4 spaces for indentation, never tabs.
*   **Line Length:** Keep line length under 120 characters where possible.
*   **`using` directives:** Group `using` directives with `System.*` namespaces first, followed by other namespaces in alphabetical order.
*   **Nullability:** Enable nullable reference types (`<Nullable>enable</Nullable>`) in the project file and use null-forgiving operators (`!`) sparingly.
*   **Asynchronous Programming:** Prefer the use of `async`/`await` patterns for asynchronous operations.

## Tests
*   Use xUnit 3, NSubstitute (mocking library) and Shouldly (assertion library) for unit tests.
*   Always use AAA pattern, with explicit comments for each one.
*   The test method names should follow the pattern: `MethodName_StateUnderTest_ExpectedBehavior` (e.g., `GetValue_KeyExists_ReturnsValue`).
*   After any change, run the tests and check if everything is still ok. 

## Post-Generation Actions

After generating or modifying code, ensure the following actions are taken:
*   Always trim trailing whitespace from all lines.
*   Ensure consistent line endings (LF on Unix, CRLF on Windows).
*   Remove any extra blank lines at the end of files.
*   Format the code according to the project's existing standards before finalizing.

## Project & Build Defaults

*   **Analyzers:** Enable analyzers and the latest analysis level in the `.csproj` file (`<EnableNETAnalyzers>true</EnableNETAnalyzers>`, `<AnalysisLevel>latest</AnalysisLevel>`).
*   **Warnings:** Treat warnings as errors (`<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`).

## Tone & Style

*   **Audience:** The code produced is for a professional enterprise environment.
*   **Language:** Use clear, concise language in comments and documentation. Avoid casual slang or overly informal tone.

