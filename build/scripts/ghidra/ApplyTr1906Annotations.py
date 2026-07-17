# Applies portable names, comments, and selected variable names to the two
# bundled TR1906 OTA images.
#
# The annotations are deliberately evidence graded. A CONFIRMED comment names
# behavior visible directly in the image. INFERRED names describe a strong
# call-graph/algorithm match but are not manufacturer symbols.
#@category MaskApp.Firmware

from ghidra.app.decompiler import DecompInterface, DecompileOptions
from ghidra.program.model.listing import CodeUnit
from ghidra.program.model.pcode import HighFunctionDBUtil
from ghidra.program.model.symbol import SourceType
import sys


def read_ascii(address_value, length):
    memory = currentProgram.getMemory()
    return "".join(
        chr(memory.getByte(toAddr(address_value + offset)) & 0xff)
        for offset in range(length)
    )


program_minimum = currentProgram.getMinAddress().getOffset()
program_maximum = currentProgram.getMaxAddress().getOffset()
if (
    program_minimum == 0x00010000
    and program_maximum == 0x0002021B
    and read_ascii(0x00017800, len("TR1906R04-10")) == "TR1906R04-10"
):
    is_r04_01_10 = False
    build_name = "TR1906R04-10"
elif (
    program_minimum == 0x00010000
    and program_maximum == 0x00020117
    and read_ascii(0x000176FC, len("TR1906R04-01-10")) == "TR1906R04-01-10"
):
    is_r04_01_10 = True
    build_name = "TR1906R04-01-10"
else:
    raise ValueError(
        "Unsupported TR1906 image: range 0x{:08x}..0x{:08x} and embedded version do not match either analyzed revision".format(
            program_minimum, program_maximum
        )
    )


def build_address(r04_10, r04_01_10=None):
    if is_r04_01_10:
        return r04_01_10 if r04_01_10 is not None else r04_10
    return r04_10


annotations = []


def annotate(
    r04_10,
    r04_01_10,
    name,
    comment,
    parameters=None,
    locals_by_old_name=None,
):
    annotations.append(
        {
            "address": build_address(r04_10, r04_01_10),
            "name": name,
            "comment": comment,
            "parameters": parameters or {},
            "locals": locals_by_old_name or {},
        }
    )


def annotate_build_specific(
    r04_10,
    r04_01_10,
    name,
    comment,
    parameters=None,
    locals_by_old_name=None,
):
    address = r04_01_10 if is_r04_01_10 else r04_10
    if address is None:
        return
    annotations.append(
        {
            "address": address,
            "name": name,
            "comment": comment,
            "parameters": parameters or {},
            "locals": locals_by_old_name or {},
        }
    )


# Descriptive names for compiler-support, controller, record-database, and
# message-plumbing routines. These intentionally avoid guessing the controller
# vendor or assigning BLE semantics that are not visible in the OTA image.
structural_annotations = (
    (0x00010908, 0x00010908, "WriteControllerRoute", "INFERRED ROLE: writes an encoded low-byte route/control value before transferring through an indirect controller continuation.", {}),
    (0x0001091C, 0x0001091C, "WriteControllerField", "CONFIRMED STRUCTURE: read-modify-writes a shifted field in a controller register.", {}),
    (0x00010980, 0x00010980, "SetRegisterBankBit", "CONFIRMED STRUCTURE: selects one of two register-bank words and sets the bit selected by the caller.", {}),
    (0x0001099C, 0x0001099C, "GetDerivedTimingRate", "INFERRED ROLE: refreshes and returns a cached rate derived from the selected timing source.", {}),
    (0x000109E0, 0x000109E0, "WriteEncodedControllerField", "CONFIRMED STRUCTURE: decodes register and field selection from a packed argument, then writes the supplied field value.", {}),
    (0x00010A80, 0x00010A80, "TestAndClearControllerFlag", "CONFIRMED STRUCTURE: tests and clears controller flag bit zero, returning -1 when it was set.", {}),
    (0x00010AA8, 0x00010AA8, "SetControllerFlag", "CONFIRMED STRUCTURE: sets controller flag bit zero.", {}),
    (0x00010AB8, 0x00010AB8, "ReserveControllerChannel", "INFERRED ROLE: validates a one-hot channel selection and records it in the controller in-use mask.", {0: "channel_mask"}),
    (0x00010AF4, 0x00010AF4, "EnableControllerChannelDependencies", "INFERRED ROLE: validates a controller channel and enables the dependency bits associated with that channel.", {0: "channel_mask"}),
    (0x00010B60, 0x00010B60, "SetControllerGlobalEnable", "CONFIRMED STRUCTURE: sets the fixed global-enable bit in the controller register block.", {}),
    (0x00010B70, 0x00010B70, "ReadControllerChannelConfiguration", "CONFIRMED STRUCTURE: converts a one-hot channel mask to an index and decodes that channel's register fields into caller storage.", {0: "channel_mask", 1: "configuration"}),
    (0x00010C2A, 0x00010C2A, "OneHotMaskToIndex", "CONFIRMED: maps one-hot masks 1 through 128 to indexes 0 through 7 and returns 8 for an invalid mask.", {0: "channel_mask"}),
    (0x00010C62, 0x00010C62, "ReadControllerChannelPriority", "INFERRED ROLE: validates a channel selection and extracts its priority-like register field.", {0: "channel_mask"}),
    (0x00010C94, 0x00010C94, "GetControllerStatusCode", "INFERRED ROLE: converts bits in the controller channel-state mask to a compact status code.", {}),
    (0x00010D68, 0x00010D68, "InitializeControllerRegisterBlock", "CONFIRMED STRUCTURE: clears controller state flags and initializes each channel register slot from fixed defaults.", {0: "controller_base"}),
    (0x00010EAA, 0x00010EAA, "TestControllerEventMask", "CONFIRMED STRUCTURE: selects one of five event-mask words and tests the bit for a one-hot channel.", {0: "channel_mask", 1: "event_index"}),
    (0x00010F0C, 0x00010F0C, "WriteControllerChannelConfiguration", "CONFIRMED STRUCTURE: validates a channel configuration and encodes its fields into the controller register records.", {0: "channel_mask", 1: "configuration"}),
    (0x00011048, 0x00011048, "WriteControllerChannelField", "CONFIRMED STRUCTURE: writes one encoded value into each selected controller-channel record.", {}),
    (0x000110A0, 0x000110A0, "SetControllerEventRouting", "CONFIRMED STRUCTURE: writes a channel mask to each selected event-routing slot.", {0: "channel_mask", 1: "event_mask"}),
    (0x00011110, 0x00011110, "ClearControllerStartFlag", "CONFIRMED STRUCTURE: clears the start bit in a fixed controller command register.", {}),
    (0x00011120, 0x00011120, "SubmitBlockingControllerCommand", "CONFIRMED STRUCTURE: submits controller operation 0x22, waits for completion, and returns the resulting error state.", {}),
    (0x000111A4, 0x000111A4, "SetControllerStartFlag", "CONFIRMED STRUCTURE: sets the start bit in a fixed controller command register.", {}),
    (0x000111B4, 0x000111B4, "SubmitBlockingControllerRead", "CONFIRMED STRUCTURE: submits a controller read request, polls for completion, and returns its result word.", {}),
    (0x00011254, 0x00011254, "SubmitBlockingControllerWrite", "CONFIRMED STRUCTURE: submits a two-argument controller write request and polls for completion.", {}),
    (0x0001131C, 0x0001131C, "UpdateMaskedRegisterBits", "CONFIRMED STRUCTURE: sets or clears only the requested mask bits in a controller register.", {0: "register_base", 1: "bit_mask", 2: "enabled"}),
    (0x0001132C, 0x0001132C, "SetPackedRegisterFields", "CONFIRMED STRUCTURE: updates selected packed two-bit fields across a pair of controller words.", {}),
    (0x0001155C, 0x00011464, "ClearTimingControllerRegisterBit", "CONFIRMED STRUCTURE: clears one fixed bit in the timing-controller register block.", {}),
    (0x0001161C, 0x00011524, "SelectTimingClockSource", "INFERRED ROLE: selects a timing-source constant from control bits and derives a rounded divider.", {}),
    (0x0001168A, 0x00011592, "ConfigureTimingDivider", "INFERRED ROLE: computes and stores prescaler mode and divider values from the derived timing rate.", {}),
    (0x00011708, 0x00011610, "UpdateTimingCalibration", "INFERRED ROLE: samples a timing source ten times, averages scaled values, and updates the cached calibration.", {}),
    (0x00011818, 0x00011720, "InitializeTimingController", "INFERRED ROLE: clears timing state, configures controller fields, and initializes the divider.", {}),
    (0x00011F44, 0x00011E4C, "WaitForControllerChannelAReady", "INFERRED ROLE: conditionally polls the first controller instance until its readiness condition changes.", {}),
    (0x00011F74, 0x00011E7C, "WaitForControllerChannelBReady", "INFERRED ROLE: conditionally polls the second controller instance until its readiness condition changes.", {}),
    (0x00011FAA, 0x00011EB2, "SetLowControllerModeBits", "CONFIRMED STRUCTURE: ORs a four-bit value into the controller mode register.", {0: "controller", 1: "mode_bits"}),
    (0x00011FB6, 0x00011EBE, "GetLowControllerStatusBits", "CONFIRMED STRUCTURE: returns the low status nibble from a controller record.", {0: "controller"}),
    (0x00011FBE, 0x00011EC6, "ConfigureControllerTimingFields", "INFERRED ROLE: derives and stores packed controller timing fields from the selected rate and divider.", {}),
    (0x00012014, 0x00011F1C, "IsControllerReady", "CONFIRMED STRUCTURE: returns the controller record's readiness bit as a boolean.", {0: "controller"}),
    (0x0001203E, 0x00011F46, "ReadControllerByte", "CONFIRMED STRUCTURE: returns the low byte of the supplied controller word.", {0: "controller"}),
    (0x00012044, 0x00011F4C, "PackControllerModeFields", "CONFIRMED STRUCTURE: packs four caller fields into one controller configuration word.", {0: "field_a", 1: "field_b", 2: "field_c", 3: "field_d", 4: "configuration"}),
    (0x00012074, 0x00011F7C, "LogFormat", "CONFIRMED: wraps the firmware's variadic formatter with its output callback and context.", {0: "format"}),
    (0x00012094, 0x00011F9C, "CountLeadingZeros32", "CONFIRMED: counts leading zero bits in a 32-bit value and returns 32 for zero.", {0: "value"}),
    (0x000120D4, 0x00011FDC, "ZeroTenByteBuffer", "CONFIRMED: clears exactly five consecutive 16-bit words.", {0: "buffer"}),
    (0x000120EE, 0x00011FF6, "CopySixBytes", "CONFIRMED: copies exactly six bytes from source to destination.", {0: "destination", 1: "source"}),
    (0x00012148, 0x00012050, "VFormatToCallback", "CONFIRMED: implements printf-style parsing and emits formatted characters through a caller callback.", {0: "format", 1: "varargs", 2: "output_context", 3: "emit_char"}),
    (0x00012570, 0x00012478, "EmitLeftPadding", "CONFIRMED: emits left-padding spaces for a right-justified formatted value.", {0: "padding_count", 1: "format_flags", 2: "output_context", 3: "emit_char"}),
    (0x00012590, 0x00012498, "EmitPadding", "CONFIRMED: emits zero or space padding according to the formatter flags.", {0: "padding_count", 1: "format_flags", 2: "output_context", 3: "emit_char"}),
    (0x00012AC4, 0x000129C0, "AllocateAndQueueVariableMessage", "CONFIRMED STRUCTURE: allocates a variable-length message, clamps its payload to 0xf9 bytes, copies the payload, and queues it.", {}),
    (0x00012D30, 0x00012C2C, "QueueFixed80ByteMessageWhenReady", "CONFIRMED STRUCTURE: when the state allows, allocates, copies, and queues a fixed 0x50-byte message.", {}),
    (0x00012D80, 0x00012C7C, "HandleState4AndQueueOneByteMessage", "INFERRED ROLE: handles controller state 4 and queues a one-byte follow-up message.", {}),
    (0x00012DD4, 0x00012CD0, "QueueFixedTenByteMessage", "CONFIRMED STRUCTURE: queues a message containing a type byte and five 16-bit fields.", {}),
    (0x00012E2C, 0x00012D28, "ApplyLinkProfileIntervalAndComplete", "INFERRED ROLE: clamps and applies a link-profile interval, then completes the pending operation with success or status 0x41.", {0: "profile_index", 1: "request"}),
    (0x00012E74, 0x00012D70, "CompleteLinkProfileValidation", "INFERRED ROLE: validates link-profile state and received length, then completes with status 0, 0x41, or 0x52.", {0: "profile_index"}),
    (0x00013848, 0x00013744, "ReleaseMessageAndNotify", "INFERRED ROLE: invokes an optional completion callback and releases the associated message object.", {}),
    (0x00013A54, 0x00013950, "SetStatusFromControllerState", "INFERRED ROLE: translates a controller state into status 0 or 0x41 and emits it.", {}),
    (0x00013BB4, 0x00013AB0, "NormalizeIdentifierTo16Bytes", "CONFIRMED STRUCTURE: normalizes a 2-, 4-, or 16-byte identifier into a 16-byte caller buffer.", {0: "identifier", 1: "identifier_size", 2: "output"}),
    (0x00013BFC, 0x00013AF8, "ResolveRecordValue", "INFERRED ROLE: locates a record by identifier and resolves its direct or indirect value and length.", {}),
    (0x00013D80, 0x00013C7C, "RegisterShortIdentifierRecord", "INFERRED ROLE: builds a short-identifier descriptor and forwards it to generic record registration.", {}),
    (0x00013DBC, 0x00013CB8, "RegisterFullIdentifierRecord", "INFERRED ROLE: builds a full 16-byte identifier descriptor and forwards it to generic record registration.", {}),
    (0x00013E00, 0x00013CFC, "RegisterRecordDefinition", "INFERRED ROLE: serializes a record definition and inserts it into an ordered record list.", {}),
    (0x00013F1C, 0x00013E18, "RegisterRecordGroup", "INFERRED ROLE: registers a larger group-like record structure through the common definition path.", {}),
    (0x00014094, 0x00013F90, "CompareRecordIdentifier", "CONFIRMED STRUCTURE: performs identifier-size-aware comparison, normalizing short forms before byte comparison.", {0: "left", 1: "left_size", 2: "right", 3: "right_size"}),
    (0x00014068, 0x00013F64, "ReadRecordFlagsIfFound", "CONFIRMED STRUCTURE: locates the record range and copies its masked flag byte to caller storage when present.", {0: "identifier", 1: "flags"}),
    (0x00014086, 0x00013F82, "CompareRecordIdentifierWithShortValue", "CONFIRMED STRUCTURE: wraps a 16-bit value as a short identifier and compares it through the generic identifier routine.", {0: "identifier", 1: "identifier_size", 2: "short_value"}),
    (0x0001415C, 0x00014058, "SerializeAndInsertRecord", "INFERRED ROLE: converts a caller record to compact storage and inserts it in identifier order.", {}),
    (0x000145D4, 0x000144D0, "QueryRecordProperties", "INFERRED ROLE: resolves a record and reports the properties supported by the requested operation mask.", {}),
    (0x0001475C, 0x00014658, "ClearRecordList", "CONFIRMED STRUCTURE: releases every node in the record list and clears its head and cache pointers.", {}),
    (0x00014784, 0x00014680, "FindRecordByIdentifier", "CONFIRMED STRUCTURE: searches the ordered record list and returns the match location plus an exact-match flag.", {}),
    (0x000147EC, 0x000146E8, "ReadRecordDescriptorValue", "INFERRED ROLE: validates a descriptor identifier and extracts its short value.", {}),
    (0x000148C8, 0x000147C4, "FindRecordRange", "CONFIRMED STRUCTURE: finds the ordered record range containing an identifier, using the cached last node when possible.", {}),
    (0x00014914, 0x00014810, "ReadResolvedRecordValue", "CONFIRMED STRUCTURE: copies inline, indirect, or transformed record data to caller storage and returns its size.", {}),
    (0x000149FC, 0x000148F8, "MatchesRecordIdentifier", "CONFIRMED STRUCTURE: tests whether a resolved record matches the requested identifier.", {}),
    (0x00014A7C, 0x00014978, "ReleaseCachedRecord", "CONFIRMED STRUCTURE: releases and clears the cached record pointer.", {}),
    (0x00014AA4, 0x000149A0, "DrainAndResetRecordQueue", "INFERRED ROLE: drains queued record operations, releases retained state, and signals the next state transition.", {}),
    (0x00015184, 0x00015080, "ValidateRecordOperation", "INFERRED ROLE: resolves record properties and validates operation support and requested length.", {}),
    (0x00015230, 0x0001512C, "GetRecordValueWithCache", "INFERRED ROLE: uses a valid cached record or resolves it and queues follow-up work when pending.", {}),
    (0x000152B4, 0x000151B0, "ReleaseRecordIfInTerminalState", "INFERRED ROLE: releases an associated record operation in either recognized terminal state.", {}),
    (0x00015BC0, 0x00015ABC, "BuildRecordReadResponse", "INFERRED ROLE: validates a record read, resolves its data, clamps it to capacity, and queues data or an error response.", {}),
)

for structural_entry in structural_annotations:
    annotate(
        structural_entry[0],
        structural_entry[1],
        structural_entry[2],
        structural_entry[3],
        structural_entry[4],
    )


# A few routines are present as standalone functions in only one revision;
# the other compiler build inlined, merged, or omitted them. Keeping these
# entries revision-specific avoids forcing false cross-build boundaries.
build_specific_structural_annotations = (
    (0x00012022, None, "IsControllerFlagSet", "CONFIRMED STRUCTURE: returns one fixed controller status bit as a boolean.", {0: "controller"}),
    (0x000120E2, None, "ZeroEightByteBuffer", "CONFIRMED: clears exactly four consecutive 16-bit words.", {0: "buffer"}),
    (0x000126F8, None, "ScheduleDelayedStateCallback", "INFERRED ROLE: cancels retained work, records state 4, schedules event 0x1a after 500 time units, and signals callback 9.", {}),
    (0x00012740, None, "ReinitializeTimingController", "CONFIRMED STRUCTURE: invokes the optional cleanup callback and reinitializes the timing controller.", {}),
    (0x00012758, None, "QueueTenByteMessageInState4", "CONFIRMED STRUCTURE: queues the fixed ten-byte message only while the shared state equals 4.", {}),
    (0x00012788, None, "BeginModeTransition", "INFERRED ROLE: runs optional cleanup, resets mode state, dispatches the selected startup path, and applies the state-2 reset guard.", {}),
    (0x00012A38, None, "TriggerControllerResetIfState2", "CONFIRMED STRUCTURE: when state equals 2, clears it, writes the reset/control register between data barriers, and waits indefinitely.", {}),
    (0x00012A64, None, "ResetEightByteState", "CONFIRMED: clears the shared eight-byte state through ZeroEightByteBuffer.", {}),
    (None, 0x000128C4, "LogEightByteValueForType4", "CONFIRMED STRUCTURE: for record type 4, logs eight consecutive bytes followed by a terminator format.", {0: "value", 1: "record_type"}),
    (None, 0x00013024, "DispatchLinkProfileEventByType", "INFERRED ROLE: resolves one of fourteen link-profile handlers by event byte and invokes the selected handler.", {0: "profile_index", 1: "event"}),
    (None, 0x00013084, "ProcessLinkProfileDataChunk", "INFERRED ROLE: validates and accounts for one link-profile data chunk, then reports progress or completes with an error status.", {0: "profile_index", 1: "chunk"}),
    (None, 0x0001493C, "DrainIndexedOperationQueue", "CONFIRMED STRUCTURE: drains the work queue at offset 0x48 for an indexed context, then resets that queue.", {0: "context_index"}),
    (None, 0x000151E4, "DispatchQueuedRecordOperation", "INFERRED ROLE: resolves a queued record-operation handler, enforces one active operation, invokes it, and drains state on completion.", {0: "profile_index", 1: "operation"}),
    (None, 0x00015280, "QueueLinkProfileIntervalUpdate", "INFERRED ROLE: allocates and posts a link-profile interval update after clamping it to the controller's supported range.", {0: "profile_index", 1: "request"}),
    (None, 0x00015B7C, "QueueLinkProfileEventPayload", "CONFIRMED STRUCTURE: allocates and posts a four-byte link-profile event payload.", {0: "profile_index", 1: "event_type", 2: "event_value", 3: "status"}),
    (None, 0x00016658, "LogEightByteValueCallback", "CONFIRMED STRUCTURE: callback wrapper that logs an eight-byte value as record type 4.", {1: "value"}),
    (None, 0x00016838, "InvokeControllerCallbackWithThirdArgument", "CONFIRMED STRUCTURE: forwards only the caller's third argument through a fixed indirect controller callback.", {2: "callback_argument"}),
    (None, 0x0001684C, "HandleLinkProfileEvent16Result", "INFERRED ROLE: handles the completion result for a pending link-profile event 0x16 and releases or retries the queued operation.", {1: "result", 2: "encoded_handle"}),
    (0x00017B60, None, "QueueLinkProfileEvent", "LOW-CONFIDENCE ROLE: constructs and posts a four-byte event for the selected link profile; the vendor event meaning is unknown.", {0: "profile_index"}),
    (0x00017D24, None, "SetLinkProfileWord", "CONFIRMED STRUCTURE: validates a profile index and stores the caller's word in its table slot.", {0: "profile_index", 1: "value"}),
    (0x00018084, None, "HandleLinkProfileControlEvent", "LOW-CONFIDENCE ROLE: converts two observed link-profile control states into queued operations and invokes the registered callback; exact vendor semantics are unknown.", {}),
    (0x0001A2BC, None, "PrepareDefaultLedFramePattern", "LOW-CONFIDENCE ROLE: prepares two twelve-element pixel regions using zero or fixed 0x7e00 values according to shared display flags.", {}),
    (0x0001A9BC, None, "InitializeDefaultLedFrame", "INFERRED ROLE: resets display flags, waits for controller readiness, prepares the default pattern, and sends the LED frame.", {}),
)

for build_specific_entry in build_specific_structural_annotations:
    annotate_build_specific(
        build_specific_entry[0],
        build_specific_entry[1],
        build_specific_entry[2],
        build_specific_entry[3],
        build_specific_entry[4],
    )


# High- and medium-confidence roles in the relocated application half. Exact
# vendor event names remain unknown, so these labels describe visible table,
# service, link-profile, cryptographic, and display behavior only.
application_structural_annotations = (
    (0x000165A0, 0x0001649C, "HandleVendorEvent1A", "LOW-CONFIDENCE ROLE: checks vendor state for event 0x1a and forwards the accepted event to the lower record-queue handler; exact event semantics are unknown.", {}),
    (0x000172A8, 0x000171A4, "GetLinkProfileField78", "CONFIRMED STRUCTURE: returns the halfword at offset 0x78 in an indexed link-profile record.", {0: "profile_index"}),
    (0x000172B8, 0x000171B4, "GetLinkProfileByte", "CONFIRMED STRUCTURE: validates a profile index and returns byte zero of its table entry.", {0: "profile_index"}),
    (0x00017300, 0x000171FC, "GetLinkProfileWord", "CONFIRMED STRUCTURE: validates a profile index and returns the selected table word.", {0: "profile_index"}),
    (0x000173C4, 0x000172C0, "ReleaseLinkProfileOperation", "INFERRED ROLE: drains queued work, releases the active indexed link-profile operation, and clears its slot.", {0: "profile_index"}),
    (0x0001796C, 0x00017868, "HasLinkProfileOperation", "INFERRED ROLE: tests whether an indexed link-profile operation exists and invokes its callback when present.", {0: "profile_index"}),
    (0x00017AAC, 0x000179A8, "CompleteLinkProfileOperation", "INFERRED ROLE: posts completion for an indexed link-profile operation, releases it, and clears its pending bit.", {0: "profile_index"}),
    (0x00017CDC, 0x00017BD8, "SetLinkProfileInterval", "INFERRED ROLE: clamps and stores the link-profile interval field, then posts the associated state event.", {0: "profile_index", 1: "interval"}),
    (0x00017DE8, 0x00017CE4, "SetLinkProfileFlag", "INFERRED ROLE: sets or clears selected bits in an indexed link-profile control word.", {0: "profile_index", 1: "flag_mask", 2: "enabled"}),
    (0x000187F4, 0x000186F0, "ZeroTenByteBufferWrapper", "CONFIRMED: delegates directly to ZeroTenByteBuffer.", {0: "buffer"}),
    (0x00018780, 0x0001867C, "DispatchMessageByOpcode", "INFERRED ROLE: resolves a queued message descriptor from its encoded opcode and releases or posts the message according to descriptor flags.", {0: "message"}),
    (0x0001886C, 0x00018768, "LookupMessageDescriptorByOpcode", "CONFIRMED STRUCTURE: splits an encoded opcode into group and id, selects one of six descriptor tables, and searches it.", {0: "opcode"}),
    (0x00018FA4, 0x00018EA0, "HandleLinkProfileStateEvent", "INFERRED ROLE: reads the two link-profile state values and completes operations whose state matches the event.", {}),
    (0x000191DC, 0x000190D8, "InitializeRadioControllerState", "INFERRED ROLE: clears controller state, toggles hardware registers, and imports selected calibration words from flash address 0x3d800.", {}),
    (0x00019350, 0x0001924C, "GetService4BHandleOffset", "CONFIRMED STRUCTURE: returns the service-0x4b start handle plus a validated relative offset below six.", {0: "offset"}),
    (0x0001936A, 0x00019266, "ValidateService4BHandle", "CONFIRMED STRUCTURE: validates a handle against the six-entry service-0x4b range and returns its relative index.", {0: "handle"}),
    (0x000194AC, 0x000193A8, "InitializeService4BContext", "INFERRED ROLE: builds a six-attribute service-0x4b context and initializes its controller state.", {}),
    (0x000196EC, 0x000195E8, "LookupAttributeContextByServiceId", "CONFIRMED STRUCTURE: selects one of two stored attribute contexts by its 16-bit service id.", {0: "service_id"}),
    (0x00019734, 0x00019630, "GetAttributePropertyByServiceId", "CONFIRMED STRUCTURE: returns the property byte associated with either stored service context.", {0: "service_id"}),
    (0x000198F4, 0x000197F0, "GetService48HandleOffset", "CONFIRMED STRUCTURE: returns the service-0x48 start handle plus a validated relative offset below thirteen.", {0: "offset"}),
    (0x0001990E, 0x0001980A, "ValidateService48Handle", "CONFIRMED STRUCTURE: validates a handle against the thirteen-entry service-0x48 range and returns its relative index.", {0: "handle"}),
    (0x00019930, 0x0001982C, "RegisterService48Context", "INFERRED ROLE: allocates and initializes a service-0x48 context and derives its handle layout.", {}),
    (0x00019AEC, 0x000199E8, "InitializeService48Context", "INFERRED ROLE: writes the fixed service-0x48 context structure used by registration.", {}),
    (0x00019B10, 0x00019A0C, "QueueService48AttributeEvent", "INFERRED ROLE: allocates a service-0x48 event, maps its type to opcode 0x12 or 0x13, copies the payload, and queues it.", {}),
    (0x00019BA4, 0x00019AA0, "ApplyControllerPowerState", "LOW-CONFIDENCE ROLE: enables and configures a hardware register block and changes one controller flag; the exact peripheral and power semantics are unknown.", {}),
    (0x00019EB8, 0x00019DB4, "UpdateDisplayIntensityLevel", "INFERRED ROLE: quantizes a percentage-like input, applies calibration and rounding, and caps the resulting display intensity at 100.", {0: "requested_level"}),
    (0x0001A058, 0x00019F54, "InitializeDisplayPeripheral", "LOW-CONFIDENCE ROLE: enables a hardware block, configures its register fields, and starts it for the display path; the exact peripheral identity is unknown.", {}),
    (0x0001A0B8, 0x00019FB4, "InitializeAesRoundKeys", "CONFIRMED: expands the configured 128-bit application key into the AES round-key schedule.", {}),
    (0x0001A288, 0x0001A184, "InitializeDisplayMode20", "CONFIRMED DISPATCH ROLE: initializes internal display mode 0x20 by clearing its pixel buffer and resetting intensity state.", {}),
    (0x0001A4F4, 0x0001A3F0, "InitializeDisplayMode04Pattern", "CONFIRMED DISPATCH ROLE: initializes display mode 4 with a quantized 24-pixel 0x7e00/0x4200 pattern.", {}),
    (0x0001A5FC, 0x0001A4F8, "TickDisplayMode20Sequence", "INFERRED ROLE: advances the eleven-state mode-0x20 sequence, sends its LED frame, and returns to that mode.", {}),
    (0x0001A90C, 0x0001A808, "ResetDisplayStateFlags", "CONFIRMED STRUCTURE: clears the two shared display-state bytes.", {}),
    (0x0001AD38, 0x0001AC34, "DecodeSixPackedBytesToTwelve", "CONFIRMED STRUCTURE: expands six packed bytes through a nibble lookup table into two six-byte outputs.", {0: "packed_input"}),
    (0x0001B00C, 0x0001AF08, "ResetAnimationRendererWrapper", "CONFIRMED: delegates directly to ResetAnimationRenderer.", {}),
    (0x0001B4D0, 0x0001B3CC, "QueueControllerStartupPacket", "LOW-CONFIDENCE ROLE: builds and queues the fixed startup packet containing the bytes 'TR'; its lower-controller meaning is unknown.", {}),
    (0x0001B584, 0x0001B480, "TransitionDisplayState1AFrom3To2", "CONFIRMED STRUCTURE: when display state 0x1a is 3, changes it to 2 and posts event 0x0d.", {}),
    (0x0001BBAC, 0x0001BAA8, "AssertControllerStartupResults", "LOW-CONFIDENCE ROLE: invokes two controller setup calls and enters the fail-stop path when either reports an error; exact setup semantics are unknown.", {}),
)

for application_entry in application_structural_annotations:
    annotate(
        application_entry[0],
        application_entry[1],
        application_entry[2],
        application_entry[3],
        application_entry[4],
    )


# Small compiler/runtime helpers. These algorithms are identical in both
# builds and retain the same addresses.
annotate(
    0x00010214,
    0x00010214,
    "UnsignedDivide32",
    "CONFIRMED: 32-bit restoring unsigned division helper.",
    {0: "dividend", 1: "divisor"},
)
annotate(
    0x00010240,
    0x00010240,
    "SignedDivide32",
    "CONFIRMED: signed 32-bit division wrapper around UnsignedDivide32.",
    {0: "dividend", 1: "divisor"},
)
annotate(
    0x0001028C,
    0x0001028C,
    "Memcpy",
    "CONFIRMED: forward byte copy with an aligned word-copy fast path; overlapping ranges are not handled.",
    {0: "destination", 1: "source", 2: "byte_count"},
)
annotate(
    0x000102B0,
    0x000102B0,
    "Memset",
    "CONFIRMED: fills a byte range with one value.",
    {0: "destination", 1: "byte_count", 2: "value"},
)
annotate(
    0x000102BE,
    0x000102BE,
    "MemsetZero",
    "CONFIRMED: zero-fills a byte range through Memset.",
    {0: "destination", 1: "byte_count"},
)
annotate(
    0x000102D4,
    0x000102D4,
    "Memcmp",
    "CONFIRMED: compares two byte ranges and returns the first unsigned-byte difference, or zero when all requested bytes match.",
    {0: "left", 1: "right", 2: "byte_count"},
)
annotate(
    0x000102C2,
    0x000102C2,
    "MemsetAndReturn",
    "CONFIRMED: fills a byte range and returns the destination pointer.",
    {0: "destination", 1: "value", 2: "byte_count"},
)
annotate(
    0x000102EE,
    0x000102EE,
    "Float32Multiply",
    "CONFIRMED: IEEE-754 binary32 multiplication helper with sign, exponent, significand, zero, and rounding handling.",
)
annotate(
    0x00010368,
    0x00010368,
    "Float64Add",
    "CONFIRMED: IEEE-754 binary64 addition/subtraction helper with magnitude ordering, alignment, normalization, and rounding.",
)
annotate(
    0x000104CC,
    0x000104CC,
    "Float64Multiply",
    "CONFIRMED: IEEE-754 binary64 multiplication helper that combines segmented significands and normalizes the result.",
)
annotate(
    0x0001059C,
    0x0001059C,
    "Float64ScaleByPowerOfTwo",
    "CONFIRMED: adjusts a binary64 exponent by a signed delta while preserving its significand and handling underflow.",
)
annotate(
    0x000105C8,
    0x000105C8,
    "UInt32ToFloat32",
    "CONFIRMED: converts an unsigned 32-bit integer to IEEE-754 binary32.",
)
annotate(
    0x000105D8,
    0x000105D8,
    "Int32ToFloat64",
    "CONFIRMED: converts a signed 32-bit integer to IEEE-754 binary64.",
)
annotate(
    0x00010600,
    0x00010600,
    "Float32ToInt32",
    "CONFIRMED: converts IEEE-754 binary32 to a signed 32-bit integer with truncation toward zero.",
)
annotate(
    0x00010634,
    0x00010634,
    "Float64ToInt32",
    "CONFIRMED: converts IEEE-754 binary64 to a signed 32-bit integer with truncation toward zero.",
)
annotate(
    0x0001067C,
    0x0001067C,
    "Float32ToFloat64",
    "CONFIRMED: widens IEEE-754 binary32 to binary64 and rebiases the exponent.",
)
annotate(
    0x000106A4,
    0x000106A4,
    "Float64ToFloat32",
    "CONFIRMED: narrows IEEE-754 binary64 to binary32 with round-to-nearest-even behavior.",
)
annotate(
    0x000106DC,
    0x000106DC,
    "UnsignedDivide64",
    "CONFIRMED: restoring unsigned 64-bit division helper; the decompiler does not preserve any secondary remainder return register.",
)
annotate(
    0x0001073C,
    0x0001073C,
    "ShiftLeft64",
    "CONFIRMED: logical 64-bit left shift across a 32-bit word boundary.",
)
annotate(
    0x0001075C,
    0x0001075C,
    "ShiftRightLogical64",
    "CONFIRMED: logical 64-bit right shift across a 32-bit word boundary.",
)
annotate(
    0x0001077E,
    0x0001077E,
    "ShiftRightArithmetic64",
    "CONFIRMED: arithmetic 64-bit right shift with high-word sign extension.",
)
annotate(
    0x000107A4,
    0x000107A4,
    "RoundToNearestEven64",
    "CONFIRMED: rounds a narrowing intermediate and clears the low bit for exact-half ties.",
)
annotate(
    0x000107B4,
    0x000107B4,
    "NormalizeAndPackFloat32",
    "CONFIRMED: normalizes a wide significand, applies binary32 exponent/range rules, and rounds to nearest even.",
)
annotate(
    0x00010826,
    0x00010826,
    "RoundToNearestEven128",
    "CONFIRMED: rounds a multiword floating-point intermediate with exact-half tie-to-even behavior.",
)
annotate(
    0x00010840,
    0x00010840,
    "NormalizeAndPackFloat64",
    "CONFIRMED: normalizes a multiword significand, handles sticky bits and exponent limits, and packs binary64.",
)
annotate(
    0x000108E4,
    0x000108E4,
    "InvokeInitRecordsThenTransfer",
    "CONFIRMED STRUCTURE: invokes 16-byte initializer records from 0x26908..0x26918, then calls EntryTransferThunk. The final target decodes as data, so startup provenance remains unresolved.",
)
annotate(
    0x00010010,
    0x00010010,
    "EntryTransferThunk",
    "CONFIRMED STRUCTURE: loads the address stored at 0x10014 and transfers with BX. The target bytes do not decompile as valid code in either image.",
)

# Firmware/application protocol boundary.
annotate(
    0x000117D8,
    0x000116E0,
    "ComputeAdditiveChecksum",
    "CONFIRMED: returns the eight-bit additive checksum of a byte range.",
    {0: "bytes", 1: "byte_count"},
    {"pcVar1": "checksum", "uVar2": "index"},
)
annotate(
    0x0001190C,
    0x00011814,
    "QueuePeripheralPacket",
    "INFERRED (high confidence): wraps a payload for the firmware message transport used by brightness and LED-frame packets.",
    {0: "payload", 1: "payload_length"},
    {"local_38": "message", "local_24": "payload_length_field"},
)
annotate(
    0x00011954,
    0x0001185C,
    "InitMaskBleService",
    "CONFIRMED: creates the FFF0 service and its encrypted command, upload, visualizer, and notification characteristics.",
)
annotate(
    0x00011A5C,
    0x00011964,
    "ParseBleCommand",
    "CONFIRMED: parses the decrypted DATS, DATCP, SPEED, SMVEW, SOUT, LIGHT, LOOP, ANIM, CLRL, IMAG, and MODE commands.",
    {0: "request"},
    {
        "bVar1": "command_value",
        "bVar2": "buffer_is_empty",
        "pcVar3": "upload_state",
        "puVar4": "received_count",
        "pcVar6": "live_view_state",
        "pcVar8": "brightness_packet",
        "uVar10": "payload_length",
        "sVar11": "expected_count",
        "uVar12": "pixel_index",
        "uVar14": "scan_index",
        "iVar15": "pixel_value",
        "cVar9": "command_or_display_mode",
    },
)
annotate(
    0x00011DE0,
    0x00011CE8,
    "HandleUploadWrite",
    "CONFIRMED: consumes one decrypted upload-characteristic block for DATS type 1 or type 2.",
    {0: "request"},
    {
        "bVar1": "payload_length",
        "bVar2": "data_index",
        "bVar6": "data_byte",
        "pcVar3": "upload_state",
        "pcVar5": "upload_buffer",
    },
)
annotate(
    0x00011EAC,
    0x00011DB4,
    "HandleVisualizerWrite",
    "CONFIRMED: handles one decrypted write to the live-visualizer characteristic; this was previously mislabeled as upload finalization.",
    {0: "request"},
    {"bVar1": "payload_length", "bVar2": "scan_index", "uVar3": "delay_index"},
)
annotate(
    0x0001940C,
    0x00019308,
    "ReceiveLogCallback",
    "INFERRED: callback registered with the BLE/message layer for received diagnostic records.",
)
annotate(
    0x00019430,
    0x0001932C,
    "GattWriteMessageCallback",
    "CONFIRMED: adapts a GATT write message into DispatchGattWrite.",
)
annotate(
    0x000199B8,
    0x000198B4,
    "DispatchGattWrite",
    "CONFIRMED: decrypts writes for service-relative attributes 2, 5, and 8, copies the length-prefixed plaintext, and dispatches the command, upload, or visualizer handler.",
    {0: "dispatcher_context", 1: "write_request"},
    {
        "cVar1": "attribute_index",
        "bVar2": "payload_length",
        "pbVar4": "decrypted_block",
        "pbVar7": "source_cursor",
        "iVar8": "copy_pairs_remaining",
    },
)
annotate(
    0x00019C34,
    0x00019B30,
    "LogExceptionStackFrame",
    "CONFIRMED: logs r0, r1, r2, r3, r12, lr, pc, and psr from an exception frame. The absent vector table prevents assigning a specific exception handler.",
    {0: "exception_frame"},
)

# AES-128 implementation. The OTA contains the algorithms and expanded-key RAM
# use, but not the source key bytes stored beyond the OTA image.
annotate(
    0x000125BC,
    0x000124C4,
    "AesAddRoundKey",
    "CONFIRMED: XORs the 16-byte AES state with one round from the expanded key schedule.",
    {0: "state", 1: "expanded_round_keys", 2: "round_index"},
    {"iVar1": "byte_index"},
)
annotate(
    0x00016284,
    0x00016180,
    "AesEncryptBlock",
    "CONFIRMED: encrypts one 16-byte block with ten AES-128 rounds and no chaining state.",
    {0: "input_block", 1: "output_block"},
    {"local_38": "state", "local_3c": "round_index", "local_24": "mixed_column"},
)
annotate(
    0x00016414,
    0x00016310,
    "AesMatrixMultiplyColumn",
    "CONFIRMED: multiplies one AES state column by a supplied GF(2^8) coefficient row.",
    {0: "coefficient_row", 1: "input_column", 2: "output_column"},
)
annotate(
    0x00018758,
    0x00018654,
    "AesGfMultiply",
    "CONFIRMED: multiplies two bytes in AES GF(2^8) using the 0x1b reduction polynomial.",
    {0: "a", 1: "b"},
    {"uVar1": "product", "iVar2": "bit_index", "uVar3": "current_a"},
)
annotate(
    0x00018ABC,
    0x000189B8,
    "AesDecryptBlock",
    "CONFIRMED: decrypts one 16-byte block with ten AES-128 inverse rounds and no chaining state.",
    {0: "input_block", 1: "output_block"},
    {"local_38": "state", "local_3c": "round_index", "local_24": "mixed_column"},
)
annotate(
    0x00018C54,
    0x00018B50,
    "AesInverseMixColumns",
    "CONFIRMED: applies the AES inverse MixColumns matrix to all four columns.",
    {0: "state"},
)
annotate(
    0x00018CAC,
    0x00018BA8,
    "AesInverseShiftRows",
    "CONFIRMED: rotates AES state rows right by their row index.",
    {0: "state"},
)
annotate(
    0x00018CEC,
    0x00018BE8,
    "AesInverseSubBytes",
    "CONFIRMED: substitutes all 16 state bytes through the AES inverse S-box.",
    {0: "state"},
)
annotate(
    0x00018DF4,
    0x00018CF0,
    "AesKeyExpansion128",
    "CONFIRMED: expands a 16-byte AES-128 key into 44 words (176 bytes) of round keys.",
    {0: "key"},
    {
        "puVar3": "expanded_key",
        "pbVar4": "rcon",
        "uVar9": "word_index",
        "iVar8": "byte_offset",
        "uVar7": "previous_word",
        "uVar6": "transformed_word",
    },
)
annotate(
    0x00019B70,
    0x00019A6C,
    "AesShiftRows",
    "CONFIRMED: rotates AES state rows left by their row index.",
    {0: "state"},
)
annotate(
    0x00019CFC,
    0x00019BF8,
    "AesSubBytes",
    "CONFIRMED: substitutes all 16 state bytes through the AES forward S-box.",
    {0: "state"},
)

# Display, animation, notification, and persistence paths.
annotate(
    0x00019E8C,
    0x00019D88,
    "UpdateAndSendLivePixel",
    "CONFIRMED: updates one of 24 RGB pixels from a short live-view packet, then transmits the complete live frame.",
    {0: "request"},
)
annotate(
    0x0001A0C8,
    0x00019FC4,
    "AdvanceAnimationLoop",
    "CONFIRMED: advances the LOOP command through the 19 built-in animation IDs and repeats the sequence.",
)
annotate(
    0x0001A3B4,
    0x0001A2B0,
    "ResetAnimationRenderer",
    "CONFIRMED: clears the shared 24-pixel render buffer and resets animation frame/timing state.",
)
annotate(
    0x0001A3DC,
    0x0001A2D8,
    "RenderBuiltinAnimationFrame",
    "CONFIRMED: decodes and transmits one packed or RGB888 built-in-animation frame on the standard six-tick cadence.",
    {0: "frame_count", 1: "packed_frames", 2: "rgb888_frames", 3: "use_rgb888"},
    {"uVar3": "source_index", "uVar4": "pixel_index", "pbVar2": "frame_index"},
)
annotate(
    0x0001A810,
    0x0001A70C,
    "ExpandRgb565ToRgb888",
    "CONFIRMED: expands one packed RGB565 color to a 24-bit RGB value through the firmware lookup table.",
    {0: "rgb565"},
)
annotate(
    0x0001AC54,
    0x0001AB50,
    "SendEncryptedNotification",
    "CONFIRMED: builds a length-prefixed 16-byte response, AES-encrypts it, and notifies characteristic index 11.",
    {0: "payload_length", 1: "payload"},
)
annotate(
    0x0001ACA0,
    0x0001AB9C,
    "EraseFlashSector",
    "CONFIRMED: unlocks flash, erases the sector containing the requested address, then relocks flash.",
    {0: "flash_address"},
)
annotate(
    0x0001ACE0,
    0x0001ABDC,
    "WriteFlashWords",
    "CONFIRMED: unlocks flash and writes a word-aligned byte range, then relocks flash.",
    {0: "flash_address", 1: "source", 2: "byte_count"},
)
annotate(
    0x0001B0C4,
    0x0001AFC0,
    "PersistUploadToFlash",
    "CONFIRMED: erases 0x3c000..0x3c800 sectors, writes the 0x600-byte type-1 upload buffer at 0x3c000, and stores eight bytes of metadata at 0x3c800.",
    {0: "upload_buffer", 1: "received_count", 2: "upload_type"},
)
annotate(
    0x0001B140,
    0x0001B03C,
    "RenderBuiltinAnimationFrameTimed",
    "CONFIRMED: renders one packed or RGB888 built-in frame after a mode-dependent delay threshold.",
    {0: "frame_count", 1: "packed_frames", 2: "rgb888_frames", 3: "use_rgb888"},
    {"uVar4": "display_mode", "uVar8": "ticks_elapsed", "pbVar2": "frame_index"},
)
annotate(
    0x0001B2FC,
    0x0001B1F8,
    "DecodeAndRenderVisualizerFrame",
    "CONFIRMED: expands live-visualizer nibbles through palette A or B into 24 pixels and transmits the frame.",
    {0: "request"},
    {
        "puVar1": "palette_index",
        "iVar2": "render_buffer",
        "uVar4": "packing_mode",
        "uVar5": "output_index",
        "uVar6": "next_output_index",
    },
)
annotate(
    0x0001B5C8,
    0x0001B4C4,
    "SetDisplayMode",
    "CONFIRMED: initializes internal display modes 1..0x20, including built-ins 5..0x17, LOOP 0x18, image 0x19, uploaded data 0x1a, and alternate modes 0x1d..0x1f.",
    {0: "display_mode"},
    {"uVar5": "pixel_index", "iVar6": "source_buffer", "iVar7": "byte_offset"},
)
annotate(
    0x0001B9C0,
    0x0001B8BC,
    "SendLedFrame",
    "CONFIRMED: serializes 24 RGB565 or RGB888 pixels into a 0x4a-byte controller packet, adds an eight-bit checksum, marks transmission pending, and queues it.",
    {0: "rgb565_pixels", 1: "rgb888_pixels", 2: "use_rgb888"},
    {"puVar2": "packet", "uVar9": "pixel_index", "uVar11": "packet_index"},
)
annotate(
    0x0001BABC,
    0x0001B9B8,
    "SendLiveLedFrame",
    "CONFIRMED: serializes and immediately queues the 24-pixel live-view frame without setting the normal pending flag.",
    {0: "rgb565_pixels", 1: "rgb888_pixels", 2: "use_rgb888"},
    {"puVar2": "packet", "uVar9": "pixel_index", "uVar11": "packet_index"},
)

# Each ANIM id has an initializer and a frame-tick wrapper. Their frame tables
# point beyond the OTA image, so the exact artwork cannot be reconstructed from
# these binaries even though the control flow is present.
for animation_id in range(19):
    r04_10_init = 0x0001AA04 + animation_id * 0x1C
    r04_01_10_init = r04_10_init - 0x104
    annotate(
        r04_10_init,
        r04_01_10_init,
        "BuiltinAnimation{:02d}_Init".format(animation_id),
        "CONFIRMED DISPATCH ROLE: initializes built-in ANIM id {} (internal display mode {}). Frame data is outside this OTA image.".format(
            animation_id, animation_id + 5
        ),
    )
    annotate(
        r04_10_init + 8,
        r04_01_10_init + 8,
        "BuiltinAnimation{:02d}_Tick".format(animation_id),
        "CONFIRMED DISPATCH ROLE: advances built-in ANIM id {} within LOOP playback.".format(
            animation_id
        ),
    )

# The image's second word contains 0x00016a01, but forcing 0x16a00 to be a
# reset handler produced a ten-byte function tail in one build and a mid-
# function body with uninitialized registers in the other. Preserve the field
# as evidence without presenting it as a validated Cortex-M reset handler.
annotate(
    0x00016A00,
    0x00016A00,
    "ImageHeaderEntryCandidate",
    "UNRESOLVED: the image header points here, but control-flow and cross-build boundaries do not support the previous Reset_Handler name. Do not treat this as a validated reset routine.",
)


def get_or_create_function(address_value):
    address = toAddr(address_value)
    function = getFunctionAt(address)
    if function is not None:
        return function

    containing = getFunctionContaining(address)
    if containing is not None:
        println(
            "WARNING: {} lies inside {}; not creating a conflicting function".format(
                address, containing.getName()
            )
        )
        return None

    disassemble(address)
    return createFunction(address, None)


renamed_functions = 0
for entry in annotations:
    function = get_or_create_function(entry["address"])
    if function is None:
        continue

    desired_name = entry["name"]
    if function.getName() != desired_name:
        function.setName(desired_name, SourceType.USER_DEFINED)
        renamed_functions += 1
    function.setComment(entry["comment"])

# Preserve the few deliberately generic symbols while documenting exactly why
# a semantic name would be misleading.
unresolved_function_comments = (
    (
        build_address(0x00016730, 0x0001662C),
        "UNRESOLVED FUNCTION BOUNDARY: the paired revisions expose incompatible short bodies here, and at least one body crosses into later code. Keep the generic name until the real entry boundary is known.",
    ),
    (
        build_address(0x0001F86C, 0x0001F768),
        "UNRESOLVED DATA/CODE BOUNDARY: EntryTransferThunk targets these bytes, but they decode as repetitive invalid code and produce decompiler p-code errors in both revisions. Do not label this as main or reset code.",
    ),
)
if is_r04_01_10:
    unresolved_function_comments += (
        (
            0x00016700,
            "UNRESOLVED DATA/CODE BOUNDARY: Ghidra discovers only a four-byte body containing bad instruction data. A semantic function name is not justified.",
        ),
    )

for unresolved_address, unresolved_comment in unresolved_function_comments:
    unresolved_function = getFunctionAt(toAddr(unresolved_address))
    if unresolved_function is not None:
        unresolved_function.setComment(unresolved_comment)

# Renaming or creating functions can expose additional parameters and locals to
# Ghidra's analyzers. Finish that analysis before opening the decompiler so a
# clean import receives the same variable annotations as a later rerun.
analyzeChanges(currentProgram)

decompiler = DecompInterface()
decompiler.setOptions(DecompileOptions())
if not decompiler.openProgram(currentProgram):
    raise RuntimeError("could not initialize the decompiler")

renamed_variables = 0
for entry in annotations:
    if not entry["parameters"] and not entry["locals"]:
        continue

    function = getFunctionAt(toAddr(entry["address"]))
    if function is None:
        continue
    current_function_name = entry["name"]

    # Committing one local can cause the next decompile to split or expose an
    # additional symbol. A bounded second pass reaches a stable result while
    # keeping reruns free of duplicate aliases.
    for variable_pass in range(2):
        result = decompiler.decompileFunction(function, 60, monitor)
        if not result.decompileCompleted():
            println(
                "WARNING: could not decompile {} for variable renames: {}".format(
                    current_function_name, result.getErrorMessage()
                )
            )
            break

        symbol_iterator = result.getHighFunction().getLocalSymbolMap().getSymbols()
        symbols = []
        while symbol_iterator.hasNext():
            symbols.append(symbol_iterator.next())
        existing_names = set(symbol.getName() for symbol in symbols)
        renamed_this_pass = 0

        for symbol in symbols:
            desired_variable_name = None
            if symbol.isParameter():
                desired_variable_name = entry["parameters"].get(
                    symbol.getCategoryIndex()
                )
            else:
                desired_variable_name = entry["locals"].get(symbol.getName())

            if (
                desired_variable_name is None
                or symbol.getName() == desired_variable_name
                or desired_variable_name in existing_names
            ):
                continue
            try:
                HighFunctionDBUtil.updateDBVariable(
                    symbol, desired_variable_name, None, SourceType.USER_DEFINED
                )
                existing_names.add(desired_variable_name)
                renamed_variables += 1
                renamed_this_pass += 1
            except:
                error = sys.exc_info()[1]
                println(
                    "WARNING: could not rename {}.{} to {}: {}".format(
                        current_function_name,
                        symbol.getName(),
                        desired_variable_name,
                        error,
                    )
                )

        if renamed_this_pass == 0:
            break

decompiler.dispose()

# Mark the suspicious startup-shaped bytes without asserting that the block is
# the hardware reset entry. This label is useful when reviewing the full
# instruction export.
startup_candidate = toAddr(0x00010008)
createLabel(startup_candidate, "StartupStubCandidate", True, SourceType.USER_DEFINED)
currentProgram.getListing().setComment(
    startup_candidate,
    CodeUnit.PLATE_COMMENT,
    "UNRESOLVED: startup-shaped Thumb sequence that sets SP and calls an init-table walker, then branches to bytes that decode as data. Execution provenance is not established.",
)

println(
    "Applied {}: {} function names/comments and {} variable names".format(
        build_name, renamed_functions, renamed_variables
    )
)
