HAL
===============

HAL専科用のサンプルコード

## Clone

```
git clone --recursive https://github.com/k-hamada/HAL.git
```

## Build

* クライアント
  * Unityで実行

* サーバー
  - Debug
  ```
  xbuild Server/Server.sln
  ```
  - Release
  ```
  xbuild /p:Configuration=Release Server/Server.sln
  ```

## Run

* クライアント
  * Unityで実行

* サーバー
  - Debug
  ```
  mono Server/bin/Debug/Server.exe
  ```
  - Release
  ```
  mono Server/bin/Release/Server.exe
  ```
