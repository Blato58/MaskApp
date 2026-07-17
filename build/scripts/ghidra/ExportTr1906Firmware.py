# Exports a complete, reviewable snapshot of a TR1906 Ghidra program.
#
# Run from analyzeHeadless with one output-directory argument. The output is
# intentionally generated under artifacts/ and is not committed.
#@category MaskApp.Firmware

from java.io import BufferedWriter, File, FileOutputStream, OutputStreamWriter
from ghidra.app.decompiler import DecompInterface, DecompileOptions
from jarray import zeros


args = getScriptArgs()
if len(args) != 1:
    raise ValueError("usage: ExportTr1906Firmware.py <output-directory>")

output_dir = File(args[0])
if not output_dir.exists() and not output_dir.mkdirs():
    raise IOError("could not create output directory: {}".format(output_dir))


def open_writer(name):
    path = File(output_dir, name)
    return BufferedWriter(OutputStreamWriter(FileOutputStream(path), "UTF-8"))


def write_line(writer, value=""):
    writer.write(str(value))
    writer.newLine()


def clean_tsv(value):
    if value is None:
        return ""
    return str(value).replace("\t", " ").replace("\r", " ").replace("\n", " ")


def sorted_functions(functions):
    result = list(functions)
    result.sort(key=lambda function: function.getEntryPoint().getOffset())
    return result


function_manager = currentProgram.getFunctionManager()
functions = sorted_functions(function_manager.getFunctions(True))

metadata = open_writer("program.txt")
write_line(metadata, "name\t{}".format(currentProgram.getName()))
write_line(metadata, "language\t{}".format(currentProgram.getLanguageID()))
write_line(metadata, "compiler\t{}".format(currentProgram.getCompilerSpec().getCompilerSpecID()))
write_line(metadata, "minimum_address\t{}".format(currentProgram.getMinAddress()))
write_line(metadata, "maximum_address\t{}".format(currentProgram.getMaxAddress()))
write_line(metadata, "function_count\t{}".format(len(functions)))
metadata.close()

inventory = open_writer("functions.tsv")
write_line(
    inventory,
    "address\tsize\tname\tsource\tcallers\tcallees\tsignature\tcomment",
)
for function in functions:
    callers = sorted_functions(function.getCallingFunctions(monitor))
    callees = sorted_functions(function.getCalledFunctions(monitor))
    write_line(
        inventory,
        "\t".join(
            (
                str(function.getEntryPoint()),
                str(function.getBody().getNumAddresses()),
                clean_tsv(function.getName()),
                clean_tsv(function.getSymbol().getSource()),
                ",".join(str(item.getEntryPoint()) for item in callers),
                ",".join(str(item.getEntryPoint()) for item in callees),
                clean_tsv(function.getPrototypeString(True, True)),
                clean_tsv(function.getComment()),
            )
        ),
    )
inventory.close()

call_graph = open_writer("callgraph.tsv")
write_line(call_graph, "caller_address\tcaller_name\tcallee_address\tcallee_name")
for function in functions:
    for callee in sorted_functions(function.getCalledFunctions(monitor)):
        write_line(
            call_graph,
            "{}\t{}\t{}\t{}".format(
                function.getEntryPoint(),
                clean_tsv(function.getName()),
                callee.getEntryPoint(),
                clean_tsv(callee.getName()),
            ),
        )
call_graph.close()

listing_export = open_writer("instructions.tsv")
write_line(listing_export, "address\tbytes\tfunction\tinstruction")
listing = currentProgram.getListing()
instructions = listing.getInstructions(True)
while instructions.hasNext():
    instruction = instructions.next()
    raw_bytes = instruction.getBytes()
    byte_text = "".join("{:02x}".format(value & 0xff) for value in raw_bytes)
    containing_function = function_manager.getFunctionContaining(instruction.getAddress())
    write_line(
        listing_export,
        "{}\t{}\t{}\t{}".format(
            instruction.getAddress(),
            byte_text,
            clean_tsv(containing_function.getName() if containing_function else ""),
            clean_tsv(instruction),
        ),
    )
listing_export.close()


def read_program_bytes():
    memory = currentProgram.getMemory()
    start = currentProgram.getMinAddress()
    size = int(currentProgram.getMaxAddress().subtract(start)) + 1
    signed = zeros(size, "b")
    memory.getBytes(start, signed)
    return start, bytearray((value if value >= 0 else value + 256) for value in signed)


def reference_functions(address):
    names = set()
    references = currentProgram.getReferenceManager().getReferencesTo(address)
    for reference in references:
        function = function_manager.getFunctionContaining(reference.getFromAddress())
        if function is not None:
            names.add("{}:{}".format(function.getEntryPoint(), function.getName()))
    return names


image_start, image_bytes = read_program_bytes()
raw_strings = open_writer("raw-strings.tsv")
write_line(raw_strings, "address\ttext\tpointer_locations\treferencing_functions")
index = 0
while index < len(image_bytes):
    value = image_bytes[index]
    if value not in (9, 10, 13) and (value < 0x20 or value > 0x7e):
        index += 1
        continue

    end = index
    while end < len(image_bytes) and (
        image_bytes[end] in (9, 10, 13) or 0x20 <= image_bytes[end] <= 0x7e
    ):
        end += 1
    if end - index < 4 or end >= len(image_bytes) or image_bytes[end] != 0:
        index = end + 1
        continue

    address = image_start.add(index)
    text_value = bytes(image_bytes[index:end]).decode("ascii", "replace")
    pointer_value = address.getOffset()
    pointer_bytes = bytearray(
        (pointer_value >> shift) & 0xff for shift in (0, 8, 16, 24)
    )
    pointer_locations = []
    functions_for_string = reference_functions(address)
    for pointer_offset in range(0, len(image_bytes) - 3):
        if image_bytes[pointer_offset:pointer_offset + 4] != pointer_bytes:
            continue
        pointer_address = image_start.add(pointer_offset)
        pointer_locations.append(str(pointer_address))
        functions_for_string.update(reference_functions(pointer_address))

    write_line(
        raw_strings,
        "{}\t{}\t{}\t{}".format(
            address,
            clean_tsv(text_value),
            ",".join(pointer_locations),
            ",".join(sorted(functions_for_string)),
        ),
    )
    index = end + 1
raw_strings.close()

decompiler = DecompInterface()
decompiler.setOptions(DecompileOptions())
decompiler.toggleCCode(True)
decompiler.toggleSyntaxTree(True)
if not decompiler.openProgram(currentProgram):
    raise RuntimeError("could not initialize the decompiler")

pseudocode = open_writer("decompiled.c")
variables = open_writer("variables.tsv")
unresolved = open_writer("unresolved-functions.txt")
write_line(
    variables,
    "function_address\tfunction_name\tkind\tname\ttype\tstorage\tfirst_use\tname_locked",
)

completed = 0
failed = 0
for index, function in enumerate(functions):
    if monitor.isCancelled():
        break
    monitor.setMessage(
        "Decompiling {} ({}/{})".format(function.getName(), index + 1, len(functions))
    )

    if function.getName().startswith("FUN_") or function.getName().startswith("thunk_FUN_"):
        write_line(unresolved, "{}\t{}".format(function.getEntryPoint(), function.getName()))

    write_line(pseudocode, "/* ========================================================================")
    write_line(
        pseudocode,
        " * {} {} ({} bytes)".format(
            function.getEntryPoint(), function.getName(), function.getBody().getNumAddresses()
        ),
    )
    if function.getComment():
        for line in function.getComment().splitlines():
            write_line(pseudocode, " * {}".format(line))
    write_line(pseudocode, " * ====================================================================== */")

    result = decompiler.decompileFunction(function, 60, monitor)
    if not result.decompileCompleted():
        failed += 1
        write_line(
            pseudocode,
            "/* Decompilation failed: {} */".format(result.getErrorMessage()),
        )
        write_line(pseudocode)
        continue

    completed += 1
    write_line(pseudocode, result.getDecompiledFunction().getC())
    write_line(pseudocode)

    high_function = result.getHighFunction()
    symbols = high_function.getLocalSymbolMap().getSymbols()
    while symbols.hasNext():
        symbol = symbols.next()
        kind = "parameter" if symbol.isParameter() else "local"
        write_line(
            variables,
            "\t".join(
                (
                    str(function.getEntryPoint()),
                    clean_tsv(function.getName()),
                    kind,
                    clean_tsv(symbol.getName()),
                    clean_tsv(symbol.getDataType()),
                    clean_tsv(symbol.getStorage()),
                    clean_tsv(symbol.getPCAddress()),
                    str(symbol.isNameLocked()).lower(),
                )
            ),
        )

decompiler.dispose()
pseudocode.close()
variables.close()
unresolved.close()

summary = open_writer("summary.txt")
write_line(summary, "functions\t{}".format(len(functions)))
write_line(summary, "decompiled\t{}".format(completed))
write_line(summary, "failed\t{}".format(failed))
summary.close()

println(
    "Exported {} functions ({} failed) to {}".format(
        completed, failed, output_dir.getAbsolutePath()
    )
)
