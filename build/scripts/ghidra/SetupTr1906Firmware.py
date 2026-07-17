# Configures a header-free TR1906 OTA image imported as a raw Cortex-M binary.
#
# Import the image at 0x00010000 with ARM:LE:32:Cortex, then run this as a
# pre-analysis script. ApplyTr1906Annotations.py should run after analysis.
#@category MaskApp.Firmware

from java.math import BigInteger
from ghidra.program.model.listing import CodeUnit
from ghidra.program.model.symbol import SourceType


expected_base = toAddr(0x00010000)
if currentProgram.getMinAddress() != expected_base:
    raise ValueError(
        "TR1906 raw image must be imported at 0x00010000; current minimum is {}".format(
            currentProgram.getMinAddress()
        )
    )

context = currentProgram.getProgramContext()
thumb_mode = context.getRegister("TMode")
if thumb_mode is not None:
    context.setValue(
        thumb_mode,
        currentProgram.getMinAddress(),
        currentProgram.getMaxAddress(),
        BigInteger.ONE,
    )

listing = currentProgram.getListing()
memory = currentProgram.getMemory()

initial_stack_address = toAddr(0x00010000)
entry_field_address = toAddr(0x00010004)
startup_candidate = toAddr(0x00010008)

# Raw Binary Loader may create code at the load address before pre-analysis
# scripts run. The first eight bytes are image-header words, so clear only that
# exact range before defining them as data.
listing.clearCodeUnits(initial_stack_address, toAddr(0x00010007), False)
createDWord(initial_stack_address)
createDWord(entry_field_address)

createLabel(
    initial_stack_address,
    "image_initial_stack_pointer",
    True,
    SourceType.USER_DEFINED,
)
createLabel(
    entry_field_address,
    "image_entry_candidate_field",
    True,
    SourceType.USER_DEFINED,
)

initial_stack = memory.getInt(initial_stack_address) & 0xffffffff
entry_field = memory.getInt(entry_field_address) & 0xffffffff
listing.setComment(
    initial_stack_address,
    CodeUnit.PLATE_COMMENT,
    "Image header word 0: plausible initial stack pointer 0x{:08x}.".format(
        initial_stack
    ),
)
listing.setComment(
    entry_field_address,
    CodeUnit.PLATE_COMMENT,
    "Image header word 1: 0x{:08x}. It points at code bytes, but cross-build function boundaries do not validate it as Reset_Handler.".format(
        entry_field
    ),
)

disassemble(startup_candidate)
createLabel(
    startup_candidate,
    "StartupStubCandidate",
    True,
    SourceType.USER_DEFINED,
)
listing.setComment(
    startup_candidate,
    CodeUnit.PLATE_COMMENT,
    "Startup-shaped Thumb sequence: loads SP, invokes an initializer-record walker, then transfers indirectly to bytes that currently decode as data. Treat as unresolved until execution provenance is established.",
)

println(
    "Configured TR1906 image {}..{} in Thumb mode; header SP=0x{:08x}, entry candidate=0x{:08x}".format(
        currentProgram.getMinAddress(),
        currentProgram.getMaxAddress(),
        initial_stack,
        entry_field,
    )
)
