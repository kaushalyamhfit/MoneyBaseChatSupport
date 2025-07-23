# MoneyBaseChatSupport Backend Project

## Project Overview

This project implements a live chat support system backend, developed in ASP.NET Core 3.1. It simulates queue-based chat handling with agent shift management, overflow logic, session inactivity detection, and round-robin assignment.

No integrations with Databases

## Features

- FIFO Chat Queue
- Agent Shifts (3x8hr)
- Capacity calculated by agent seniority
- Chat request rejection if queue is full (with overflow support)
- Background monitor for inactivity (3 missed polls)
- Polling endpoint to keep sessions alive
- Status endpoint to check chat session status
- Configurable logic via `appsettings.json`
- Swagger UI for easy testing
- Centralized logging using `ILogger<T>`

## API

Method	

POST	/api/chat/request	Submit a new chat request

POST	/api/chat/poll/{chatId}	Simulate polling from client

GET	/api/chat/status/{chatId}	Check chat session status

## Configurations

"ChatConfig": {
  "MaxQueueMultiplier": 1.5,
    "OfficeHoursStart": "09:00",
    "OfficeHoursEnd": "18:00",
      "SeniorityEfficiency": {
          "Junior": 0.4,
          "MidLevel": 0.6,
          "Senior": 0.8,
          "TeamLead": 0.5
          }
        }

## Folder Structure

Controllers/         → API endpoints
Services/            → ChatQueueService and PollMonitor
Models/              → Agent, ChatSession, Enums
Configuration/       → Strongly typed settings from appsettings.json
