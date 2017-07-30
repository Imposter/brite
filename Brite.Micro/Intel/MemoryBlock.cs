﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brite.Micro.Intel
{
    public class MemoryBlock
    {
        // CS & IP registers for 80x86 systems.

        /// <summary>
        /// Code Segment register (16-bit).
        /// </summary>
        public ushort Cs { get; set; }

        /// <summary>
        /// Instruction Pointer register (16-bit).
        /// </summary>
        public ushort Ip { get; set; }

        // EIP register for 80386 and higher CPU's.

        /// <summary>
        /// Extended Instruction Pointer register (32-bit).
        /// </summary>
        public uint Eip { get; set; }

        /// <summary>
        /// Returns the index of the highest modified cell.
        /// </summary>
        public int HighestModifiedOffset
        {
            get { return Cells.LastIndexOf(cell => cell.Modified); }
        }

        /// <summary>
        /// Returns the size of this memory, in bytes.
        /// </summary>
        public int MemorySize => Cells.Length;

        /// <summary>
        /// Memory cells in this memory block.
        /// </summary>
        public MemoryCell[] Cells { get; set; }


        /// <summary>
        /// Construct a new MemoryBlock.
        /// </summary>
        /// <param name="memorySize">The size of the MemoryBlock to instantiate.</param>
        /// <param name="fillValue">Default cell initialization / fill value.</param>
        public MemoryBlock(int memorySize, byte fillValue = 0xff)
        {
            Cells = new MemoryCell[memorySize];
            for (var i = 0; i < memorySize; i++)
                Cells[i] = new MemoryCell(i) { Value = fillValue };
        }
    }
}
