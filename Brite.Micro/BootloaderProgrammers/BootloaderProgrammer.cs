using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brite.Micro.Hardware;
using Brite.Micro.Hardware.Memory;
using Brite.Micro.Intel;
using Brite.Utility.IO;

namespace Brite.Micro.BootloaderProgrammers
{
    public abstract class BootloaderProgrammer : IBootloaderProgrammer
    {
        private static readonly Log log = Logger.GetLog<BootloaderProgrammer>();

        public abstract Task Open();
        public abstract Task Close();
        public abstract Task EstablishSync();
        public abstract Task CheckDeviceSignature();
        public abstract Task InitializeDevice();
        public abstract Task EnableProgrammingMode();
        public abstract Task LeaveProgrammingMode();
        public abstract Task LoadAddress(IMemory memory, int offset);
        public abstract Task ExecuteWritePage(IMemory memory, int offset, byte[] bytes);
        public abstract Task<byte[]> ExecuteReadPage(IMemory memory);

        protected IMicrocontroller MCU { get; private set; }

        protected BootloaderProgrammer(IMicrocontroller mcu)
        {
            MCU = mcu;
        }

        public virtual async Task ProgramDevice(MemoryBlock memoryBlock)
        {
            var sizeToWrite = memoryBlock.HighestModifiedOffset + 1;
            var flashMem = MCU.Flash;
            var pageSize = flashMem.PageSize;
            log.Info("Preparing to write {0} bytes...", sizeToWrite);
            log.Info("Flash page size: {0}.", pageSize);

            int offset;
            for (offset = 0; offset < sizeToWrite; offset += pageSize)
            {
                var needsWrite = false;
                for (var i = offset; i < offset + pageSize; i++)
                {
                    if (!memoryBlock.Cells[i].Modified) continue;
                    needsWrite = true;
                    break;
                }
                if (needsWrite)
                {
                    log.Debug("Executing paged write @ address {0} (page size {1})...", offset, pageSize);
                    var bytesToCopy = memoryBlock.Cells.Skip(offset).Take(pageSize).Select(x => x.Value).ToArray();

                    log.Trace("Checking if bytes at offset {0} need to be overwritten...", offset);
                    await LoadAddress(flashMem, offset);
                    var bytesAlreadyPresent = await ExecuteReadPage(flashMem);
                    if (bytesAlreadyPresent.SequenceEqual(bytesToCopy))
                    {
                        log.Trace("Bytes to be written are identical to bytes already present - skipping actual write!");
                        continue;
                    }
                    log.Trace("Writing page at offset {0}.", offset);
                    await LoadAddress(flashMem, offset);
                    await ExecuteWritePage(flashMem, offset, bytesToCopy);

                    log.Trace("Page written, now verifying...");
                    await Task.Delay(10);
                    await LoadAddress(flashMem, offset);
                    var verify = await ExecuteReadPage(flashMem);
                    var succeeded = verify.SequenceEqual(bytesToCopy);
                    if (!succeeded)
                    {
                        var message = "Difference encountered during verification, write failed!";
                        log.Error(message);
                        throw new Exception(message);
                    }
                }
                else
                {
                    log.Trace("Skip writing page...");
                }
            }
            log.Info("{0} bytes written to flash memory!", sizeToWrite);
        }
    }
}
