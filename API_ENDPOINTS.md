# API Endpoints Checklist

## Cars Management

GET /api/cars - [x] Implemented

## Tracks Management

GET /api/tracks - [x] Implemented

## Setup & Configuration

POST /api/setup/nick - [x] Implemented
GET /api/setup/nick - [x] Implemented
POST /api/setup/car - [x] Implemented
GET /api/setup/car - [x] Implemented
POST /api/setup/track - [x] Implemented
GET /api/setup/track - [x] Implemented
POST /api/setup/shifter - [x] Implemented
GET /api/setup/shifter - [x] Implemented
GET /api/setup/summary - [x] Implemented

## Controls & Input Binding

GET /api/controls/devices - [x] Implemented
POST /api/controls/bind - [x] Implemented (obsługuje wiele bindów dla GEARUP/GEARDN oraz bindowanie biegów GEAR_1-GEAR_7 i GEAR_R)
POST /api/controls/bind/h-shifter - [x] Implemented
POST /api/controls/bind/sequential - [x] Implemented
POST /api/controls/save - [x] Implemented
GET /api/controls/state - [x] Implemented
GET /api/controls/bindings - [x] Implemented (pokazuje wszystkie bindy jeden pod drugim)
POST /api/controls/load - [x] Implemented
DELETE /api/controls/bind/{actionName} - [x] Implemented (usuwa wszystkie bindy dla akcji)
DELETE /api/controls/bind - [x] Implemented (usuwa konkretny bind przez ID)

## Video Settings

POST /api/video/display-mode - [x] Implemented
POST /api/video/resolution - [x] Implemented
POST /api/video/save - [x] Implemented
GET /api/video/state - [x] Implemented
POST /api/video/load - [x] Implemented

## Content Management

POST /api/content/upload - [x] Implemented
POST /api/content/upload-assetofolder - [x] Implemented
POST /api/content/upload-car - [x] Implemented
POST /api/content/upload-track - [x] Implemented

## Game Launch

POST /api/game/launch - [x] Implemented

