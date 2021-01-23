using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Net.Sockets;
using System.Threading;
using System.Security.Cryptography;

public class TCPConnectionController : MonoBehaviour{
    public Text logText;
    public Text passwordInputText;
    public GameObject virtualKeyboard;
    public GameObject desktopPlane;
    public PlaneCurveController customMeshController;
    public bool useMeshController;

    private String displayString = "";
    private Texture2D desktopImageTexture;
    private bool desktopTextureReady = false;
    private bool desktopRefreshRequested = false;
    private bool desktopPlaneActive = false;
    private bool desktopPlaneActiveRequested = false;
    public KeyboardInputHandler keyboardInputHandler;

    private Color32[] desktopPixelColors;
    private int receivedPixelWidth = 0;
    private int receivedPixelHehight = 0;
    private int requestScreenWidth = 1920;
    private int requestScreenHeight = 1080;
    private bool screenSizeDetermined = false;
    private bool textureInitialized = false;
    private bool vncConnectionReady = false;
    private int keyboardInputType = 0;
    private string vncServerHostIPADdress = "";
    private int vncServerHostPort = 5900;
    private bool tcpConnectionEstablished = false;
    private bool rfbConnectionEstablished = false;
    private bool desktopTextureRefreshRequested = false;

    public bool testMode = false;
    public bool shouldSendPointerEvent = true; // true in Production

    private TcpClient vncTcpClient;
    private Thread vncConnectionThread;
    public GameObject pointerObject;
    public Button disconnectButton;
    public LineRenderer OVRSelectionVisualizer;

    private bool pointerEventRequested = false;
    private Vector2 cursorScreenPosition;
    private byte pointerEventType = 0;
    private bool disconnectButtonActiveUpdateRequested = false;
    private Byte[] passwordChallenge = new Byte[16];

    private VNCUtils.VNCController vncController;

    private Thread pointerEventThread;

    public Slider screenPositionSlider;
    public Slider screenCurvatureSlider;

    private float initialScreenZ;
    public float maxScreenZ = 10.0f;
    public float minScreenZ = 2.0f;

    private int loopCount = 0;
    private Vector2 previousTouchpadPosition;
    private bool touchpadGestureActive = false;
    public int bytesPerPixel = 4;
    private bool requestPasswordInputFieldFlush = false;

    private SynchronizationContext UIThreadContext;
    public Action<Action<string>> onPasswordRequestedDelegate = null;
    public Action onLoginOK = null;
    //private int previousFrameUpdateMillisec = 0;
    private System.DateTime previousFrameUpdateTime;
    void Start(){
        UIThreadContext = SynchronizationContext.Current;
        keyboardInputHandler.onEnterDelegate = (()=>{
            onPasswordKeyboardEnter();
        });
        previousFrameUpdateTime = System.DateTime.Now;
        UnityEngine.XR.XRSettings.eyeTextureResolutionScale=2.0f;
        //TNDebug.Log("Start()");
        disconnectButton.interactable = false;
        initDistanceSliderValue();
        if (testMode)
            startTCPConnection();
        //disconnectButton.SetActive(false);
        //disconnectButton.interactable = false;
        initialScreenZ = desktopPlane.transform.position.z;
    }
    private void initDistanceSliderValue(){
        // 初期設定に合わせた値にスライダーをセット
        if (useMeshController){
            float sliderValue = 1.0f-(2.0f - minScreenZ)/(maxScreenZ-minScreenZ);
            //TNDebug.Log("sliderValue=" + sliderValue);
            screenPositionSlider.value = sliderValue;
        } else {
            float sliderValue = 1.0f-(desktopPlane.transform.position.z - minScreenZ)/(maxScreenZ-minScreenZ);
            //TNDebug.Log("sliderValue=" + sliderValue);
            screenPositionSlider.value = sliderValue;
        }
    }
    private void listenForTCPServer(){
        try {
            //TcpClient socket = new TcpClient("192.168.0.4", 5901);
            //Security.PrefetchSocketPolicy("127.0.0.1", 5901);
            //TcpClient socket = new TcpClient("localhost", 5900);
            //TcpClient socket = new TcpClient("192.168.0.4", 5900);
            //vncServerHostIPADdress = "192.168.0.4";
            //vncServerHostPort = 5900;
            if (testMode) {
                //vncServerHostIPADdress = "192.168.0.4";
                vncServerHostIPADdress = "127.0.0.1";
                //vncServerHostIPADdress = "192.168.0.4";
                //vncServerHostIPADdress = "192.168.0.4";
                vncServerHostPort = 5900;
            }
            TNDebug.Log("connecting to server host:");
            TNDebug.Log(vncServerHostIPADdress);
            TNDebug.Log("port:");
            TNDebug.Log(vncServerHostPort);
            vncController = new VNCUtils.VNCController();
            vncController.initSocket(
                vncServerHostIPADdress, vncServerHostPort);
            TcpClient socket = vncController.socketTcpClient;
            //authenticate(socket);
            vncTcpClient = socket;
            Byte[] authType = vncController.authenticate();
            handleAuthenticateType(authType);
            while (!vncConnectionReady){}
            //setPixelEncodings(socket);
            //requestPixelBufferUpdate(socket, 0, 0, 0, requestScreenWidth, requestScreenHeight);
            //vncController.setEncodings(new int[]{16});
            vncController.setEncodings(new int[]{5, 0});
            if (bytesPerPixel == 2){
                vncController.setPixelFormat(
                                             16, 16,
                                             15, 15, 15,
                                             8, 4, 0);
            } else if (bytesPerPixel == 4){
                vncController.setPixelFormat(
                    32, 24,
                    255, 255, 255,
                    16, 8, 0);
            }
            vncController.requestPixelBufferUpdate(
                0, 0, 0,
                //64, 64
                requestScreenWidth, requestScreenHeight
                );
            receivedPixelHehight = requestScreenHeight;
            receivedPixelWidth = requestScreenWidth;
            NetworkUtil.BufferedNetworkStream bufferedStream = new NetworkUtil.BufferedNetworkStream();
            bufferedStream.initBuffedBytes(1920 * 1080 * 4);
            bufferedStream.setNetworkStream(socket.GetStream());

            // Thread parsePixelBufferUpdateThread = new Thread(new ThreadStart(()=>{
            //     NetworkUtil.BufferedNetworkStream bufferedStream = new NetworkUtil.BufferedNetworkStream();
            //     bufferedStream.initBuffedBytes(1920 * 1080 * 4);
            //     bufferedStream.setNetworkStream(socket.GetStream());
            //     while (rfbConnectionEstablished){
            //         parsePixelBufferUpdateResult(bufferedStream);
            //     }
            // }));
            //parsePixelBufferUpdateThread.Start();
            parsePixelBufferUpdateResult(bufferedStream);
            int loopCount = 0;
            //TNDebug.Log("entering loop");
            // if (testMode){
            //     vncController.sendScroll(-2, new Vector2(300, 200));
            // }
            System.DateTime lastFrameUpdateRequestTime = System.DateTime.Now;
            while (true){
                if (!rfbConnectionEstablished){
                    // Disconnected 
                    TNDebug.Log("disconnecting @ rfbConnectionEstablished");
                    break;
                }
                while((System.DateTime.Now-lastFrameUpdateRequestTime).TotalMilliseconds <= 50){
                }
                //setPixelEncodings(socket);
                //TNDebug.Log("requesting frame update");
                //requestPixelBufferUpdate(socket, 1, 0, 0, requestScreenWidth, requestScreenHeight);
                //vncController.setEncodings(new int[]{5});
                vncController.requestPixelBufferUpdate(
                    1, 0, 0, 
                    requestScreenWidth, requestScreenHeight
                    //64, 64
                );
                lastFrameUpdateRequestTime = System.DateTime.Now;
                // incremental
                //parsePixelBufferUpdateResult(socket);
                parsePixelBufferUpdateResult(bufferedStream);
                                // 描写が終わるまで待つ
                //TNDebug.Log("pixel buffer received");
                // if (pointerEventRequested && !testMode){
                //     TNDebug.Log("pointer event");
                //     sendPointerEvent(pointerEventType, cursorScreenPosition);
                //     pointerEventRequested = false;
                // }
                //while(desktopTextureReady){}
            }
        }
        catch (SocketException exception){
            //logText.text = "socket error";
            TNDebug.Log(exception);
            TNDebug.Log("socket error");
            //displayString = "Socket error";
            //displayString = exception.ToString();
            displayString = "Network error occured. VNC server host name:";
            keyboardInputType = 0; // expect IP host name
        }
        catch (Exception otherException){
            TNDebug.Log("disconnected due to Error");
            TNDebug.Log("other exceptions");
            TNDebug.Log(otherException.ToString());
        }
    }
    public void startVNCConnection(string hostName, int portName){
        vncServerHostIPADdress = hostName;
        vncServerHostPort = portName;
        startTCPConnection();
    }
    private void startFrameSync(){
    }

    public void sendPointerClick(Vector2 clickPosition){
        sendPointerDown(clickPosition);
        sendPointerUp(clickPosition);
    }
    public void sendPointerDown(Vector2 clickPosition){
        writeToSOcket(
            vncTcpClient, getPointerEventBytes(1, clickPosition)
        );
    }

    public Byte[] getPointerEventBytes(Byte buttonMask, Vector2 pointerPosition){
        return VNCUtils.VNCController.getPointerEventBytes(buttonMask, pointerPosition);
    }

    public void sendPointerUp(Vector2 clickPosition){
        writeToSOcket(
            vncTcpClient, getPointerEventBytes(0, clickPosition)
        );
    }
    private void sendPointerEvent(byte eventType, Vector2 clickPosition){
        writeToSOcket(vncTcpClient, getPointerEventBytes(eventType, clickPosition));
    }

    private void writeToSOcket(TcpClient client, Byte[] data){
        NetworkStream stream = client.GetStream();
        stream.Write(data, 0, data.Length);
    }

    private void sendVNCVersion(TcpClient tcpClient){
        //tcpClient.send("RFB 003.003\n");
        NetworkStream stream = tcpClient.GetStream();
        Byte[] data = Encoding.ASCII.GetBytes("RFB 003.003\n");
        stream.Write(data, 0, data.Length);
    }
    private void handleAuthenticateType(Byte[] receivedContent){
        int authType = receivedContent[3];
        TNDebug.Log("authtype");
         if (authType == 2){
            TNDebug.Log("Password required");
            if (!testMode){
                //logText.text = "password required";
                displayString = "password required";
            }
            for (int i=0; i<16; i++){
                passwordChallenge[i] = receivedContent[i+4];
            }
            //vncTcpClient = tcpClient;
            requestPasswordInput();
        } else if (authType == 0){
            // no password required
            // TODO go to serverinit
            if (!testMode){
                logText.text = "Execpected authentication method. Please use password authentication.";
            }
            keyboardInputType = 0;
            // vncTcpClient = tcpClient;
            // rfbConnectionEstablished = true;
            // disconnectButtonActiveUpdateRequested = true;
            //     desktopPlaneActiveRequested = true;
            // getScreenInformation(vncTcpClient);
        }
    }
    private void requestPasswordInput(){
        TNDebug.Log("requesting password");
        keyboardInputType = 2; // Waiting for password
        if (onPasswordRequestedDelegate != null){
            onPasswordRequestedDelegate((pw)=>{
                //Debug.Log("sending password");
                sendPassword(pw);
            });
            return;
        }
        if (!testMode){
           //passwordInputText.text = "";
           requestPasswordInputFieldFlush = true;
        } 
        if (testMode){
            sendPassword();
        }
    }
    public void onPasswordKeyboardEnter(){
        if (keyboardInputType == 0){
            vncServerHostIPADdress = passwordInputText.text;
            keyboardInputType = 1;
            logText.text = "VNC server port:";
            passwordInputText.text = "";
        }
        else if (keyboardInputType == 1){
             vncServerHostPort = Int32.Parse(passwordInputText.text);
             TNDebug.Log("VNC server port");
             TNDebug.Log(vncServerHostPort);
             keyboardInputType = 2;
             logText.text = "Password:";
             startTCPConnection();
        }
        else if (keyboardInputType == 2){
            sendPassword();
        }
    }
    private void startTCPConnection(){
         try {
            TNDebug.Log("thread preparing");
            vncConnectionThread = new Thread(new ThreadStart(listenForTCPServer));
            vncConnectionThread.IsBackground = true;
            vncConnectionThread.Start();
            TNDebug.Log("thread start");
            logText.text = "Connecting...";
        } catch (Exception e){
            TNDebug.Log(e);
            //logText.text = "thread error";
            logText.text = "failed to open TCP";
        }
    }

    private void sendPassword(){
        string password = passwordInputText.text;
        if (testMode){
            password = "vncpass";
        }
        sendPassword(password);
    }
    private void sendPassword(string password){
        Byte[] encryptedPassword = EncryptPasswordWithChallenge(password, passwordChallenge);
        try{
            writeToSOcket(vncTcpClient, encryptedPassword);
        } catch (SocketException e){
            TNDebug.Log("socket error @ sendPassword");
            displayString = "Socket error, failed to send the password";
        }
        //authenticateResult(vncTcpClient);
        int authResult = vncController.authenticationResult();
        handleAuthenticateResult(authResult);
        //getScreenInformation(vncTcpClient);
        vncController.getScreenInformation();
        requestScreenWidth = vncController.requestScreenWidth;
        requestScreenHeight = vncController.requestScreenHeight;
        initializeScreenTexture();
        vncConnectionReady = true;
    }
    private void runOnUIThread(Action action){
        //StartCoroutine(runInLateUpdate(action));
        UIThreadContext.Post(__ =>{
            action();
        }, null);
    }
    IEnumerator runInLateUpdate(Action action){
        yield return null;
        action();
    }

    private void handleAuthenticateResult(int authenticateResult){
        if (authenticateResult == 0){
                TNDebug.Log("Login successful");
                if (!testMode){
                    //logText.text = "login OK";
                    displayString = "login OK";
                }
                if (onLoginOK != null){
                    //Debug.Log("calling onLoginOK");
                    runOnUIThread(()=>{onLoginOK();});
                } else {
                    //Debug.Log("no onLoginOK");
                }
                rfbConnectionEstablished = true;
                disconnectButtonActiveUpdateRequested = true;
                desktopPlaneActiveRequested = true;
                //virtualKeyboard.SetActive(false);
                //disconnectButton.SetActive(true);
            } else {
                //logText.text = "login failure";
                displayString = "Login failure";
                TNDebug.Log("login failure");
            }
            if (!testMode){
                runOnUIThread(()=>{
                    passwordInputText.text = "";
                });
            }
    }
    private void setPixelEncodings(TcpClient client){
        Byte[] setPixelEncodingsByte = new Byte[4];
        setPixelEncodingsByte[0] = 1;
        setPixelEncodingsByte[1] = 0;
        setPixelEncodingsByte[2] = 0;
        setPixelEncodingsByte[3] = 1; // send one encoding -- RAW encoding
        writeToSOcket(client, setPixelEncodingsByte);
        setPixelEncodingsByte[0] = 0;
        setPixelEncodingsByte[1] = 0;
        setPixelEncodingsByte[2] = 0;
        setPixelEncodingsByte[3] = 1; // RAW encoding --> 0x00
        writeToSOcket(client, setPixelEncodingsByte);
        TNDebug.Log("sending encoding");
    }
    private void initializeScreenTexture(){
        screenSizeDetermined = true;
        textureInitialized = false;
        desktopPixelColors = new Color32[requestScreenHeight*requestScreenWidth];
        desktopTextureRefreshRequested = true;
    }

    //void parsePixelBufferUpdateResult(TcpClient client){
    void parsePixelBufferUpdateResult(NetworkUtil.BufferedNetworkStream bufferedStream){
        //Debug.Log("parsePixelBufferUpdateResult start: " + System.DateTime.Now.Millisecond);
        //desktopTextureReady = false;
        Byte[] data = new Byte[12];
        //NetworkStream stream = client.GetStream();
        int length = 0;
        // length = stream.Read(data, 0, 4);
        // TNDebug.Log("length (expect 4)" + length);
        //data = VNCUtils.VNCController.streamReadTillEnd(stream, 4);
        data = bufferedStream.ReadStream(4);
    
        int numberOfRectangles = data[2] << 8 | data[3];
        System.DateTime nowTime = System.DateTime.Now;
        int fps = 1000/(int)((nowTime-previousFrameUpdateTime).TotalMilliseconds);
        TNDebug.Log("FPS=" + fps.ToString());
        TNDebug.Log("time from previous update=" + (nowTime-previousFrameUpdateTime).TotalMilliseconds.ToString());
        //previousFrameUpdateMillisec = startFrameUpdateTime;
        previousFrameUpdateTime = nowTime;
        int bufferIndex;
        int rectangleX;
        int rectangleY;
        int rectangleH;
        int rectangleW;
        int rectangleNumberOfBytes;
        int encoding;
        //Debug.Log("frame update start processing=" + System.DateTime.Now.Millisecond);
        for (int i=0; i<numberOfRectangles; i++){
            //length = stream.Read(data, 0, 12);
            //TNDebug.Log("length (expect 12)=" + length);
            //data = streamReadTillEnd(stream, 12);
            //data = VNCUtils.VNCController.streamReadTillEnd(stream, 12);
            data = bufferedStream.ReadStream(12);
            bufferIndex = 0;
            rectangleX = data[bufferIndex] << 8 | data[bufferIndex+1];
            rectangleY =  (data[bufferIndex+2] << 8 | data[bufferIndex + 3]);
            rectangleW = (data[bufferIndex+4] << 8 | data[bufferIndex+5]);
            rectangleH = (data[bufferIndex+6] << 8 | data[bufferIndex+7]);
            encoding = (data[bufferIndex+8] << 24 | data[bufferIndex+9] << 16 | data[bufferIndex+10] << 8 | data[bufferIndex+11]);
            TNDebug.V("encoding=" + encoding.ToString());
            if (encoding == 5){
                PixelEncodeDecoder.HextileDecoder decoder = new PixelEncodeDecoder.HextileDecoder();
                PixelEncodeDecoder.HextileDecoder.decodeToColors
                    (bufferedStream,
                     desktopPixelColors,
                     rectangleX, rectangleY,
                     rectangleW, rectangleH,
                     bytesPerPixel,
                     requestScreenWidth, requestScreenHeight);
                if (i == numberOfRectangles-1){
                    //Debug.Log("frame update finish processing=" + System.DateTime.Now.Millisecond);
                    desktopRefreshRequested = true;
                }
            } else if (encoding == 0){
                rectangleNumberOfBytes = rectangleH * rectangleW * 4;
                Byte[] rectangleData = new Byte[rectangleNumberOfBytes];
                int currentPositionIndex = 0;
                int currentColorIndex = 0;
                byte currentRed = 0;
                byte currentGreen = 0;
                byte currentBlue = 0;
                int currentColorPositionIndex = 0;

                //while ((length = stream.Read(rectangleData, 0, rectangleNumberOfBytes)) != 0){;
                //rectangleData = bufferedStream.ReadStream(rectangleNumberOfBytes);
                while((length = bufferedStream.CompatRead(rectangleData, 0, rectangleNumberOfBytes)) != 0){
                    if (!rfbConnectionEstablished){
                        TNDebug.Log("break in frame rectangle loop");
                        break;
                    }
                    //TNDebug.Log("received rectangle length=" + length);
                    //TNDebug.Log("remaining length=" + rectangleNumberOfBytes);
                    bufferIndex = 0;
                    if (encoding == 0){
                        // RAW Encoding
                        while (currentColorPositionIndex < rectangleH * rectangleW * 4){
                            if (bufferIndex >= length){
                                //TNDebug.Log("buffer length processed");
                                break;
                            }
                            //currentPositionIndex = currentColorPositionIndex/4;
                            //TNDebug.Log("X=" + positionX.ToString() + ":Y=" + positionY.ToString());
                            if (currentColorPositionIndex % 4 == 0){
                                //currentBlue = (float)(rectangleData[bufferIndex]/255.0);
                                currentBlue = rectangleData[bufferIndex];
                            } else if (currentColorPositionIndex % 4 == 1){
                                //currentgreen = (float)(rectangleData[bufferIndex]/255.0);
                                currentGreen = rectangleData[bufferIndex];
                            } else if (currentColorPositionIndex % 4 == 2){
                                //currentRed = (float)(rectangleData[bufferIndex]/255.0);
                                currentRed = rectangleData[bufferIndex];
                            } else if (currentColorPositionIndex % 4 == 3){
                                int positionY = currentPositionIndex/rectangleW;
                                int positionX = currentPositionIndex - positionY * rectangleW;
                                int colorArrayIndex = receivedPixelWidth * (rectangleY + positionY) + receivedPixelWidth - rectangleX - positionX-1;
                                //int colorArrayIndex = 0;
                                desktopPixelColors[colorArrayIndex] = new Color32(currentRed, currentGreen, currentBlue, 255);
                                currentPositionIndex ++;
                            }
                            //currentPositionIndex ++;
                            bufferIndex ++;
                            currentColorPositionIndex ++;
                        }
                        rectangleNumberOfBytes -= length;
                        if (rectangleNumberOfBytes <= 0){
                            //TNDebug.Log("rectangle processed");
                            desktopRefreshRequested = true;
                            break;
                        }
                    } else {
                        displayString = "The encoding from the server is not supported: " + encoding.ToString();
                    }
                }
            } else {
                throw new Exception("invalid encoding:" + encoding.ToString());
            }
        }
        desktopTextureReady = true;
        TNDebug.Log("image received @ time" + (System.DateTime.Now-nowTime).TotalMilliseconds);
    }

    private void moveScreenY(){
        float newZValue = screenPositionSlider.value * minScreenZ + (1-screenPositionSlider.value) * maxScreenZ;
        //TNDebug.Log("moving scren Z");
        //TNDebug.Log(newZValue.ToString());
        //TNDebug.Log("current value: " + desktopPlane.transform.position.z);
        if (useMeshController){
            customMeshController.changeCurvatureZ(newZValue);
            changeScreenCurvature();
        } else{
             if (desktopPlane.transform.position.z != newZValue){
                //desktopPlane.transform.position.z = newZValue;
                desktopPlane.transform.position = new Vector3(
                    desktopPlane.transform.position.x,
                    desktopPlane.transform.position.y,
                    newZValue
                );
            } else {
                //TNDebug.Log("same z coordinate");
            }
        }
    }

    private void changeScreenCurvature(){
        float minScreenRadius = 3.0f;
        float maxScreenRadius = 50.0f;
        float newScreenRadius = minScreenRadius * screenCurvatureSlider.value + maxScreenRadius * (1-screenCurvatureSlider.value);
        if (useMeshController){
            customMeshController.changeCurvatureRadius(newScreenRadius);
        }
    }

    public void onSliderValueChanged(float newValue){
        float newZValue = newValue * minScreenZ + (1-newValue) * initialScreenZ;
        if (desktopPlane.transform.position.z != newZValue){
             //desktopPlane.transform.position.z = newZValue;
             desktopPlane.transform.position = new Vector3(
                 desktopPlane.transform.position.x,
                 desktopPlane.transform.position.y,
                 newZValue
             );
        }
    }

    // Update is called once per frame
    void Update(){
        if (requestPasswordInputFieldFlush){
            passwordInputText.text = "";
            requestPasswordInputFieldFlush = false;
        }
        moveScreenY();
        if (desktopPlaneActiveRequested != desktopPlaneActiveRequested){
            desktopPlane.SetActive(desktopPlaneActiveRequested);
            desktopPlaneActive = desktopPlaneActiveRequested;
        }
        if (screenSizeDetermined){
            if (!textureInitialized){
                desktopImageTexture = new Texture2D(requestScreenWidth, requestScreenHeight, TextureFormat.RGBA32, false);   
                desktopImageTexture.filterMode = FilterMode.Point;
                textureInitialized = true;
                if (useMeshController){
                    customMeshController.initMeshWithSize(
                        requestScreenWidth, requestScreenHeight
                    );
                } else {
                    Vector3 planeScale = desktopPlane.transform.localScale;
                    planeScale.x = (float)(requestScreenWidth)/(float)(requestScreenHeight) * planeScale.z;
                    desktopPlane.transform.localScale = planeScale;
                }
                TNDebug.Log("texture initialized");
            }
            // if (desktopPlaneActiveRequested){
            //     desktopPlaneActive.SetActive(true);
            //     desktopPlaneActiveRequested = false;
            // }
        }
        if (disconnectButtonActiveUpdateRequested){
            //disconnectButton.SetActive(true);
            disconnectButton.interactable = true;
            virtualKeyboard.SetActive(false);
            disconnectButtonActiveUpdateRequested = false;
        }
        //logText.text = displayString;
        if (desktopTextureReady){
            //TNDebug.Log("desktop texture ready");
            if (desktopRefreshRequested){
                desktopImageTexture.SetPixels32(desktopPixelColors);
                desktopImageTexture.Apply();
                desktopRefreshRequested = false;
                //TNDebug.Log("drawing new image");
            } else {
                //TNDebug.Log("desktop texture not ready");
            }
            // if (!desktopPlaneActf    ive){
            //     desktopPlane.SetActive(true);
            //     desktopPlaneActive = true;
            //     //desktopPlane.GetComponent<Renderer>().material.mainTexture = desktopImageTexture;
            //     //desktopPlane.GetComponent<Renderer>().material.SetTexture("_EmissionMap", desktopImageTexture);
            // }
            if (desktopTextureRefreshRequested){
                desktopTextureRefreshRequested = false;
                //TNDebug.Log("changing emission map");
                if (useMeshController){
                    customMeshController.setTexture(desktopImageTexture);
                } else {
                    desktopPlane.GetComponent<Renderer>().material.SetTexture("_EmissionMap", desktopImageTexture);
                }
            }
            //TNDebug.Log("image to Plane");
            //desktopPlane.GetComponent<Renderer>().material.mainTexture = desktopImageTexture;
            //UnityEngine.XR.XRSettings.eyeTextureResolutionScale=2.0f;
            //desktopTextureReady = false;
        }
        if (rfbConnectionEstablished){
            if (useMeshController){
                if (!customMeshController.focusOnScreen()){
                    // pointer is focused on other panel.
                    return;
                }
                Vector2 relativePointerPosition = customMeshController.getPointerPosition();
                cursorScreenPosition.x = requestScreenWidth * relativePointerPosition.x;
                cursorScreenPosition.y = requestScreenHeight * relativePointerPosition.y;
            } else {
                showPointerLocation();
                cursorScreenPosition = getScreenPositionFromCursorPosition(pointerObject.transform.position);
            }
           
            if (!isScreenPositionInsideScreen(cursorScreenPosition)){
                //sendPointerDown(cursorScreenPosition);
                return;
            }
            if (pointerEventRequested){
                // 前のポインターイベント処理が終わっていない
                return;
            }
            if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger)){
                //sendPointerDown(cursorScreenPosition);
                //pointerPosition = cursorScreenPosition;
                pointerEventRequested = true;
                pointerEventType = 1;
            } else if (OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger)){
                //sendPointerUp(cursorScreenPosition);
                //pointerPosition = cursorScreenPosition;
                pointerEventRequested = true;
                pointerEventType = 0;
            } else if (OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger)){
                //sendPointerDown(cursorScreenPosition);
                //pointerPosition = cursorPointPosition;
                pointerEventRequested = true;
                pointerEventType = 1;
            } else {
                //sendPointerUp(cursorScreenPosition);
                //pointerPosition = cursorPointPosition;
                pointerEventRequested = true;
                pointerEventType = 0;
            }
            if (pointerEventRequested){
                asyncSendPointerEvent();
                pointerEventRequested = false;
            }
        }
        if (!String.IsNullOrEmpty(displayString)){
            logText.text = displayString;
            displayString = "";
        }
        if (OVRInput.GetUp(OVRInput.Button.Back)){
            OVRManager.PlatformUIConfirmQuit();
        }
        handleScrollEventOnTouchpad();
    }
    private void handleScrollEventOnTouchpad(){
        if (!shouldSendPointerEvent){
            return;
        }
        bool touchpadTouched = OVRInput.Get(OVRInput.Touch.PrimaryTouchpad);
        Vector2 touchpadPosition = OVRInput.Get(OVRInput.Axis2D.PrimaryTouchpad);
        if (!touchpadTouched){
            touchpadGestureActive = false;
            return;
        }
        if (touchpadGestureActive){
            Vector2 touchpadPositionDelta = touchpadPosition - previousTouchpadPosition;
            int scrollDistance = (int)(5 * touchpadPositionDelta.y);
            vncController.sendScroll(scrollDistance, cursorScreenPosition);
        }
        previousTouchpadPosition = touchpadPosition;
        touchpadGestureActive = true;
    }
    private void asyncSendPointerEvent(){
        Thread pointerEventThread = new Thread(new ThreadStart(sendPointer));
        pointerEventThread.IsBackground = true;
        pointerEventThread.Start();
    }
    private void sendPointer(){
        if (shouldSendPointerEvent)
            sendPointerEvent(pointerEventType, cursorScreenPosition);
    }
    private bool isScreenPositionInsideScreen(Vector2 screenPosition){
        // check whether cursor position is inside the screen
        int positionInScreenX = (int)screenPosition.x;
        int positionInScreenY = (int)screenPosition.y;
        if (positionInScreenX >= 0 && positionInScreenX < requestScreenWidth && positionInScreenY >= 0 && positionInScreenY < requestScreenHeight){
            //logText.text = positionInScreenY.ToString();
            return true;
            //sendPointerClick(screenPosition);
        } else {
            return false;
        }
    }
    private Vector2 getScreenPositionFromCursorPosition(Vector3 cursorPosition){
        // convert physical cursor position (3d) into scaled screen cursor position(2d)
        float pointerRawX = cursorPosition.x;
        float pointerRawY = cursorPosition.y;
        Mesh desktopPlaneMesh = desktopPlane.GetComponent<MeshFilter>().mesh;
        float absoluteDesktopPlaneHeight = desktopPlaneMesh.bounds.size.z * desktopPlane.transform.localScale.z; //90ド回転
        float absoluteDesktopPlaneWidth = desktopPlaneMesh.bounds.size.x * desktopPlane.transform.localScale.x;
        int positionInScreenX = (int)(requestScreenWidth * (absoluteDesktopPlaneWidth/2+pointerRawX-desktopPlane.transform.position.x)/absoluteDesktopPlaneWidth);
        int positionInScreenY = (int)(requestScreenHeight * (absoluteDesktopPlaneHeight/2 - pointerRawY+desktopPlane.transform.position.y)/absoluteDesktopPlaneHeight);
        //TNDebug.Log(positionInScreenX);
        Vector2 screenPosition = new Vector2(positionInScreenX, positionInScreenY);
        return screenPosition;
    }
    private void showPointerLocation(){
        Vector3[] selectionVisualizerPositions = new Vector3[OVRSelectionVisualizer.positionCount];
        OVRSelectionVisualizer.GetPositions(selectionVisualizerPositions);
        Vector3 intersectionPoint = getIntersectionOfOVRRayAtZ(
                selectionVisualizerPositions[0],
                selectionVisualizerPositions[1], 
                desktopPlane.transform.position.z);
        pointerObject.transform.position = intersectionPoint;
    }

    private byte ReverseBits(byte inputByte){
        byte outputByte = inputByte;
        inputByte >>= 1;
        for (int i=0; i<7; i++){
            outputByte <<= 1;
            outputByte |= (byte)(inputByte & 1);
            inputByte >>= 1;
        }
        return outputByte;
    }
    private Byte[] EncryptPasswordWithChallenge(string password, Byte[] challenge){
        Byte[] passwordByte = new Byte[8];
        if (password.Length >= 8){
            Encoding.ASCII.GetBytes(password, 0, 8, passwordByte, 0);
        } else {
            Encoding.ASCII.GetBytes(password, 0, password.Length, passwordByte, 0);
        }
        for (int i=0; i<8; i++){
            byte passwordByteReversed = ReverseBits(passwordByte[i]);
            passwordByte[i] = passwordByteReversed;
        }

        DES des = new DESCryptoServiceProvider();
        des.Padding = PaddingMode.None;
        des.Mode = CipherMode.ECB;
        ICryptoTransform cryptoTransform = des.CreateEncryptor(passwordByte, null);
        Byte[] response = new Byte[16];
        cryptoTransform.TransformBlock(challenge, 0, challenge.Length, response, 0);
        return response;
    }
    private Vector3 getIntersectionOfOVRRayAtZ(Vector3 visualizerPosition1, Vector3 visualizerPosition2, float z){
        // vector (visualizerPosition1 ---> visualizerPosition2) と平面z=zの交点
        Vector3 lineDirection = visualizerPosition2-visualizerPosition1;
        if (lineDirection.z == 0){
            // 交差なし
            return new Vector3(0, 0, 0);
        }
        float lineDirectionCoefficient = (z-visualizerPosition1.z)/(lineDirection.z);
        Vector3 intersectPoint = visualizerPosition1 + lineDirectionCoefficient * lineDirection;
        return intersectPoint;
    }
    public void onConnectionCloseButtonClicked(){
        TNDebug.Log("VNC disconnect");
        virtualKeyboard.SetActive(true);
        //disconnectButton.SetActive(false);
        displayString = "";
        if (!testMode){
            logText.text = "Current connection closed. Restart the app for a new connection.";
        }
        vncTcpClient.Close();
        vncConnectionThread.Interrupt();
        //desktopPlane.SetActive(false);
        rfbConnectionEstablished = false;
        //desktopPlaneActive = false;
        //keyboardInputType = 0;
        desktopPlaneActiveRequested = false;
    }
}
