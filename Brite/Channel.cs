﻿/*
 * Copyright (C) 2017 Eyaz Rehman. All Rights Reserved.
 *
 * This file is part of Brite.
 * Licensed under the GNU General Public License. See LICENSE file in the project
 * root for full license information.
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Brite.Utility;
using Brite.Utility.IO;

namespace Brite
{
    public class Channel
    {
        private readonly byte _index;
        private readonly ushort _maxSize;
        private readonly byte _maxBrightness;
        private readonly byte _animationMaxColors;
        private readonly float _animationMinSpeed;
        private readonly float _animationMaxSpeed;
        private readonly TypedStream _stream;
        private readonly Mutex _streamLock;
        private readonly int _retries;
        private readonly List<uint> _supportedAnimations;

        private ushort _size;
        private byte _brightness;
        private Animation _animation;

        // Properties
        public byte Index => _index;
        public ushort MaxSize => _maxSize;
        public byte MaxBrightness => _maxBrightness;
        public ushort Size => _size;
        public byte Brightness => _brightness;
        public Animation Animation => _animation;

        internal Channel(TypedStream stream, Mutex streamLock, int retries, byte index, ushort maxSize, byte maxBrightness, byte animationMaxColors, float animationMinSpeed, float animationMaxSpeed, List<uint> supportedAnimations)
        {
            _stream = stream;
            _streamLock = streamLock;
            _retries = retries;

            _index = index;
            _maxSize = maxSize;
            _maxBrightness = maxBrightness;
            _animationMaxColors = animationMaxColors;
            _animationMinSpeed = animationMinSpeed;
            _animationMaxSpeed = animationMaxSpeed;
            _supportedAnimations = supportedAnimations;
        }

        public async Task SetSizeAsync(ushort size)
        {
            // Ensure that the size is within the limit
            size = Math.Min(size, _maxSize);

            // Store size
            _size = size;

            // Notify device
            try
            {
                // Lock mutex
                await _streamLock.LockAsync();

                // Wait for device to get ready
                await SendCommandAsync(Command.SetChannelLedCount);

                // Send parameters
                _stream.TypesEnabled = true;
                await _stream.WriteUInt8Async(_index);
                await _stream.WriteUInt16Async(size);

                // Read response
                _stream.TypesEnabled = true;
                var result = await _stream.ReadUInt8Async();
                if (result != (byte)Result.Ok)
                    throw new Exception("Unable to set channel size");
            }
            finally
            {
                _streamLock.Unlock();
            }
        }

        public async Task SetBrightnessAsync(byte brightness)
        {
            // Ensure that the size is within the limit
            brightness = brightness.Clamp((byte)0, _maxBrightness);

            // Store brightness
            _brightness = brightness;

            // Notify device
            try
            {
                // Lock mutex
                await _streamLock.LockAsync();

                // Wait for device to get ready
                await SendCommandAsync(Command.SetChannelBrightness);

                // Send parameters
                _stream.TypesEnabled = true;
                await _stream.WriteUInt8Async(_index);
                await _stream.WriteUInt8Async(brightness);

                // Read response
                _stream.TypesEnabled = true;
                var result = await _stream.ReadUInt8Async();
                if (result != (byte)Result.Ok)
                    throw new Exception("Unable to set channel brightness");
            }
            finally
            {
                _streamLock.Unlock();
            }
        }

        public async Task SetAnimationAsync(Animation animation, bool reset = true)
        {
            // Check if the animation is supported
            var animId = animation.GetId();
            if (!_supportedAnimations.Contains(animId))
                throw new NotSupportedException("Animation is not supported");

            // Reset previous animation
            _animation?.Reset();

            // Store animation
            _animation = animation;

            // Initialize animation
            _animation.Initialize(_stream, _streamLock, _retries, reset, _index, _animationMaxColors, _animationMinSpeed, _animationMaxSpeed);

            // Notify device
            try
            {
                // Lock mutex
                await _streamLock.LockAsync();

                // Wait for device to get ready
                await SendCommandAsync(Command.SetChannelAnimation);

                // Send parameters
                _stream.TypesEnabled = true;
                await _stream.WriteUInt8Async(_index);
                await _stream.WriteUInt32Async(animId);

                // Read response
                _stream.TypesEnabled = true;
                var result = await _stream.ReadUInt8Async();
                if (result != (byte)Result.Ok)
                    throw new Exception("Unable to set channel animation");
            }
            finally
            {
                _streamLock.Unlock();
            }
        }

        public async Task ResetAsync()
        {
            if (_animation == null)
                return;

            // Reset animation
            _animation.Reset();

            // Set animation to null
            _animation = null;

            // Notify device
            try
            {
                // Lock mutex
                await _streamLock.LockAsync();

                // Wait for device to get ready
                await SendCommandAsync(Command.SetChannelAnimation);

                // Send parameters
                _stream.TypesEnabled = true;
                await _stream.WriteUInt8Async(_index);
                await _stream.WriteUInt32Async(0);

                // Read response
                _stream.TypesEnabled = true;
                var result = await _stream.ReadUInt8Async();
                if (result != (byte)Result.Ok)
                    throw new Exception("Unable to reset channel");
            }
            finally
            {
                _streamLock.Unlock();
            }
        }

        private async Task SendCommandAsync(Command command)
        {
            var typesEnabled = _stream.TypesEnabled;

            for (var i = 0; i < _retries; i++)
            {
                try
                {
                    _stream.TypesEnabled = false;
                    await _stream.WriteUInt8Async((byte)command);

                    var response = await _stream.ReadUInt8Async();
                    if (response == (byte)command)
                        break;
                }
                catch (Exception ex)
                {
                    if (i == _retries - 1)
                        throw new Exception("Unable to send command", ex);
                }
            }

            _stream.TypesEnabled = typesEnabled;
        }
    }
}
