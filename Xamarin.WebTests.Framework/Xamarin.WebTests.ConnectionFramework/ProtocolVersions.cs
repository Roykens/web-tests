﻿//
// ProtocolVersion.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2015 Xamarin, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;

namespace Xamarin.WebTests.ConnectionFramework
{
	[Flags]
	public enum ProtocolVersions
	{
		None		= 0,
		Unspecified	= 0,

		Tls10Server	= 0x0040,
		Tls10Client	= 0x0080,
		Tls10		= (Tls10Server|Tls10Client),

		Tls11Server	= 0x0100,
		Tls11Client	= 0x0200,
		Tls11		= (Tls11Server|Tls11Client),

		Tls12Server	= 0x0400,
		Tls12Client	= 0x0800,
		Tls12		= (Tls12Server|Tls12Client),

		ServerMask	= (Tls10Server|Tls11Server|Tls12Server),
		ClientMask	= (Tls10Client|Tls11Client|Tls12Client)
	}
}

