#ifndef CC_TYPEDEFS_H
#define CC_TYPEDEFS_H
/* Ensures variables are of a fixed size.
   Copyright 2017 ClassicalSharp | Licensed under BSD-3
*/

#if _MSC_VER
typedef unsigned __int8  UInt8;
typedef unsigned __int16 UInt16;
typedef unsigned __int32 UInt32;
typedef unsigned __int64 UInt64;

typedef signed __int8  Int8;
typedef signed __int16 Int16;
typedef signed __int32 Int32;
typedef signed __int64 Int64;
#elif (__STDC_VERSION__ >= 199901L)
#include <stdint.h>
typedef uint8_t  UInt8;
typedef uint16_t UInt16;
typedef uint32_t UInt32;
typedef uint64_t UInt64;

typedef int8_t Int8;
typedef int16_t Int16;
typedef int32_t Int32;
typedef int64_t Int64;
#else
#error "I didn't add typedefs for this compiler. You'll need to define them in Typedefs.h!"
#endif

typedef float  Real32;
typedef double Real64;

typedef UInt8 bool;
#define true 1
#define false 0
#define NULL 0

#if USE16_BIT
typedef UInt16 BlockID;
#else
typedef UInt8 BlockID;
#endif
typedef UInt8 EntityID;
typedef UInt8 TextureLoc;
/* Sides of a block. TODO: Map this to CPE PlayerClicked blockface enums. */
typedef UInt8 Face;
typedef UInt32 ReturnCode;

typedef struct FontDesc_ { void* Handle; UInt16 Size, Style; } FontDesc;

#define UInt8_MaxValue   ((UInt8)255)
#define Int16_MinValue   ((Int16)-32768)
#define Int16_MaxValue   ((Int16)32767)
#define UInt16_MaxValue  ((UInt16)65535)
#define Int32_MinValue   ((Int32)-2147483647L - (Int32)1L)
#define Int32_MaxValue   ((Int32)2147483647L)
#define UInt32_MaxValue  ((UInt32)4294967295UL)

#define USE_DX false

#if USE_DX
typedef void* GfxResourceID;
#else
typedef UInt32 GfxResourceID;
#endif
#endif