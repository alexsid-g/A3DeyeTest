# File Generator and Sorter

## Test File Generator

The generator writes newline-delimited records in `<number>. <string>` format. Numbers are random non-negative 64-bit integers, and strings are random slices of its built-in word corpus. Output is buffered and streamed rather than accumulated in memory.

The requested file size is a minimum: generation stops after writing the line that reaches or exceeds that size, so the final file can be slightly larger. Sizes use binary units (`MB` = 1024² bytes, `GB` = 1024³ bytes).

```sh
dotnet run --project TestFileGenerator -- -o test-data.txt -s 10MB
```

Options:

- `-o` — output file path (required).
- `-s` — target size, in `MB` or `GB` (required).
- `--fast` — use fast algorithm for generation (optional).

Each generated line has the required format, but the generator does not guarantee unique strings or a specified number of duplicates.

Implemented generator tasks:

| ID | Implemented behavior |
| --- | --- |
| G1 | Defines the `<number>. <string>` record format. |
| G2 | Generates a random non-negative 64-bit number for each record. |
| G3 | Streams output through a buffered file stream. |

## File Sorter

The sorter reads records as dot-delimited CSV without a header. The first field must be a signed 64-bit integer and the second field is treated as a string. Results are ordered by string ascending, then number ascending; duplicate records are retained. DuckDB reads and writes the data, so output is serialized as CSV using the same delimiter rather than preserving the original line bytes.

```sh
dotnet run --project FileSorter -- -i test-data.txt -o sorted-data.txt
```

Options:

- `-i` — input file path (required; must exist).
- `-o` — output file path (required by the current strategy implementation).
- `-m` — optional memory limit in `MB` or `GB`. When provided, the sorter selects its simple or disk-backed strategy using an estimate of 1.3 times the input size. The large-file strategy passes the limit to DuckDB.
- `-d` — optional DuckDB temporary directory for the large-file strategy. It defaults to the current working directory; provide an existing directory.
- `-sa` - optional sorting algorithm for large files (the one you cannot keep in memory). One of []. 

The large-file path uses DuckDB's disk-backed database and temporary storage. The inactive Parquet split-and-merge path is not part of the current behavior, and no specific maximum supported file size is guaranteed.

Implemented sorter tasks:

| ID | Implemented behavior |
| --- | --- |
| S1 | Uses DuckDB CSV parsing to read the number and string fields. |
| S2 | Orders records by string ascending. |
| S3 | Uses number ascending as the secondary sort key. |
| S4 | Uses DuckDB's disk-backed strategy and temporary storage when selected. |
| S5 | Writes sorted output as dot-delimited CSV without a header. |
