#ifndef CC_CHAT_H
#define CC_CHAT_H
#include "Constants.h"
#include "Utils.h"
#include "GameStructs.h"
/* Manages sending and logging chat.
   Copyright 2014-2017 ClassicalSharp | Licensed under BSD-3
*/

#define MSG_TYPE_NORMAL         0
#define MSG_TYPE_STATUS_1       1
#define MSG_TYPE_STATUS_2       2
#define MSG_TYPE_STATUS_3       3
#define MSG_TYPE_BOTTOMRIGHT_1  11
#define MSG_TYPE_BOTTOMRIGHT_2  12
#define MSG_TYPE_BOTTOMRIGHT_3  13
#define MSG_TYPE_ANNOUNCEMENT   100
#define MSG_TYPE_CLIENTSTATUS_1 256 /* Cuboid messages*/
#define MSG_TYPE_CLIENTSTATUS_2 257 /* Clipboard invalid character */
#define MSG_TYPE_CLIENTSTATUS_3 258 /* Tab list matching names*/

typedef struct ChatLine_ { UInt8 Buffer[String_BufferSize(STRING_SIZE)]; DateTime Received; } ChatLine;
ChatLine Chat_Status[3], Chat_BottomRight[3], Chat_ClientStatus[3], Chat_Announcement;
StringsBuffer Chat_Log, Chat_InputLog;

IGameComponent Chat_MakeGameComponent(void);
void Chat_SetLogName(STRING_PURE String* name);
void Chat_Send(STRING_PURE String* text);
void Chat_Add(STRING_PURE String* text);
void Chat_AddOf(STRING_PURE String* text, Int32 messageType);
#define Chat_AddRaw(str, raw) String str = String_FromConst(raw); Chat_Add(&str);
#endif