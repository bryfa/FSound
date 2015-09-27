﻿//
// FSound - F# Sound Processing Library
// Copyright (c) 2015 by Albert Pang <albert.pang@me.com> 
// All rights reserved.
//
// This file is a part of FSound
//
// FSound is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// FSound is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
namespace FSound

module Utilities =

  open MathNet.Numerics.IntegralTransforms
  open FSound.IO
  open FSound.Signal
  open FSound.Play

  ///
  /// <summary>Folding with an index</summary>
  /// <param name="f">function which takes a state, an integer which is the
  /// index of the element in the sequence, the element itself and returns a new
  /// state</param>
  /// <param name="acc">initial state</param>
  /// <param name="xs">list of elements to be folded</param>
  /// <returns></returns>
  ///
  let foldi f acc xs = 
    let rec foldi' f i acc xs =
      match xs with
      | [] -> acc
      | h::t -> foldi' f (i+1) (f acc i h) t
    foldi' f 0 acc xs

  ///
  /// <summary>Naive implementation of the discrete fourier transform. Use at
  /// your own peril - it does not perform well and only amplitude is calculated
  /// </summary>
  /// <param name="samples">list of samples</param>
  /// <returns>list of frequency component amplitudes</returns>
  ///
  let naiveDft samples =
 
    let dftComponent k s =
      let N = Seq.length s
      let w = 2.0*System.Math.PI*(float k)/(float N)
      foldi (fun (re,im) i x-> (re + x*cos(w*(float i)), 
                                im + x*sin(w*(float i)))) 
                                (0.0, 0.0) s
    
    Seq.mapi (fun i _ -> dftComponent i samples) samples

  ///
  /// <summary>Wrapper for the MathNet.Numerics (3.7.0) fourier transform.
  /// First convert the float samples to System.Numerics.Complex.  Then
  /// call MathNet.Numerics.IntegralTransforms.Fourier.Forward which modifies
  /// the input inline</summary>
  /// <param name="samples">sequence of real float samples</param>
  /// <returns>complex array</returns>
  ///
  let fft samples =

    let cmplxSamples = 
      samples 
      |> Seq.map (fun x -> System.Numerics.Complex(x, 0.0))
      |> Seq.toArray

    Fourier.Forward(cmplxSamples)
    cmplxSamples

  ///
  /// <summary>Returns magnitude of a complex number</summary>
  /// <param name="c">a complex number</param>
  /// <returns>Magnitude of the given complex number</returns>
  ///
  let magnitude (c:System.Numerics.Complex) = c.Magnitude

  ///
  /// <summary>Returns the seq of magnitudes of a seq of complex numbers
  /// </summary>
  /// <param name="cs">sequence of complex numbers</param>
  /// <returns>sequence of magnitudes of sequence of complex numbers</returns>
  ///
  let magnitudes (cs:seq<System.Numerics.Complex>) = Seq.map magnitude cs

  ///
  /// <summary>Returns the phase of a complex number</summary>
  /// <param name="c">a complex number</param>
  /// <returns>Phase of the given complex number</returns>
  ///
  let phase (c:System.Numerics.Complex) = c.Phase

  ///
  /// <summary>Returns the seq of phases of a seq of complex numbers
  /// </summary>
  /// <param name="cs">sequence of complex numbers</param>
  /// <returns>sequence of phases of sequence of complex numbers</returns>
  ///
  let phases (cs:seq<System.Numerics.Complex>) = Seq.map phase cs

  ///
  /// <summary>Returns the magnitude and phase of a complex number</summary>
  /// <param name="c">a complex number</param>
  /// <returns>A pair containing the magnitude and phase of the given complex
  /// number</returns>
  ///
  let toPolar (c:System.Numerics.Complex) = (magnitude c, phase c)
  
  ///
  /// <summary>Convenience function to generate a wav file with the supplied 
  /// wave function which is of compact disc parameters i.e. 44100Hz sampling 
  /// rate and 16-bit sample. Only one channel is created</summary>
  /// <param name="duration">number of seconds</param>
  /// <param name="filename">filename of the output wav file</param>
  /// <param name="waveform">the waveform function</param>
  ///
  let wavCd1 duration filename waveform =
    waveform
    |> generate 44100.0 duration
    |> floatTo16
    |> makeSoundFile 44100.0 1 16 true
    |> toWav filename

  ///
  /// <summary>Yet another convenience function to play a wave function for a
  /// given duration in seconds, just to save some typing</summary>
  /// <param name="sf">sampling frequency</param>
  /// <param name="duration">duration in number of seconds</param>
  /// <param name="waveFunc">the waveform function which takes a time t as
  /// argument and return a sample</param>
  /// <returns>unit</returns>
  ///
  let playWave sf duration waveFunc =
    waveFunc
    |> generate sf duration
    |> floatTo16
    |> makeSoundFile sf 1 16 true
    |> playSoundFile