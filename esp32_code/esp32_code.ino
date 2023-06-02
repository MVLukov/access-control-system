#include <Ticker.h>
#include <WiFi.h>
#include <HTTPClient.h>
#include <WiFiClientSecure.h>
#include <Arduino.h>
#include <AsyncHTTPSRequest_Generic.h>

//rc522
#include <SPI.h>
#include <MFRC522.h>

#define SS_PIN 5
#define RST_PIN 0

MFRC522 rfid(SS_PIN, RST_PIN);  // Instance of the class

//set led pin for errors
#define ERR_LED 4
#define SUCC_LED 2

Ticker blinker;
AsyncHTTPSRequest request;

String id = "";
const char *ssid = "WIFI SSID"; //WIFI SSID
const char *password = "WIFI PASSWORD"; //WIFI PASSWORD
bool newCard = false;
bool authorized = false;
bool notAuthorized = false;
bool isWiFiConnected = false;

void setup() {
  pinMode(ERR_LED, OUTPUT);
  pinMode(SUCC_LED, OUTPUT);
  Serial.begin(115200);

  //config wifi
  configWifi();

  //config rc522
  SPI.begin();      // Init SPI bus
  rfid.PCD_Init();  // Init MFRC522

  char ssid[12];
  snprintf(ssid, 12, "%llX", ESP.getEfuseMac());
  id = String(ssid[2]) + String(ssid[3]) + String(ssid[0]) + String(ssid[1]);
  Serial.println(id);

  request.onReadyStateChange(requestCB);
}

void WiFiDisconnected(WiFiEvent_t event) {
  Serial.println("WiFi disconnected!");
  isWiFiConnected = false;
  blinker.attach_ms(250, blinkLED);
}

void WiFiGotIP(WiFiEvent_t event, WiFiEventInfo_t info) {
  Serial.println("WiFi connected");
  Serial.println("IP address: ");
  Serial.println(IPAddress(info.got_ip.ip_info.ip.addr));
  isWiFiConnected = true;
  blinker.detach();
}

void configWifi() {
  WiFi.mode(WIFI_STA);
  WiFi.onEvent(WiFiDisconnected, WiFiEvent_t::ARDUINO_EVENT_WIFI_STA_DISCONNECTED);
  WiFi.onEvent(WiFiGotIP, WiFiEvent_t::ARDUINO_EVENT_WIFI_STA_GOT_IP);

  WiFi.begin(ssid, password);
}

void blinkLED() {
  digitalWrite(ERR_LED, !digitalRead(ERR_LED));
}

void sendRequest(String rfid) {
  static bool requestOpenResult;
  String url = "https://{server ip}:7165/Home/AuthorizeTag?tagId=" + rfid + "&" + "deviceId=" + id;

  if (request.readyState() == readyStateUnsent || request.readyState() == readyStateDone) {
    requestOpenResult = request.open("GET", url.c_str());

    if (requestOpenResult) {
      request.send();
    } else {
      Serial.println(F("Can't send bad request"));
    }
  } else {
    Serial.println(F("Can't send request"));
  }
}

void requestCB(void *optParm, AsyncHTTPSRequest *request, int readyState) {
  (void)optParm;

  if (readyState == readyStateDone) {
    int statusCode = request->responseHTTPcode();

    Serial.println(F("\n**************************************"));
    Serial.println(statusCode);
    Serial.println(request->elapsedTime());
    Serial.println(F("**************************************"));

    digitalWrite(SUCC_LED, LOW);
    digitalWrite(ERR_LED, LOW);

    if (statusCode == 200) {
      digitalWrite(SUCC_LED, HIGH);
      authorized = true;
    } else {
      digitalWrite(ERR_LED, HIGH);
      notAuthorized = true;
    }
    request->setDebug(true);
  }
}

void loop() {
  if (isWiFiConnected) {
    if (authorized || notAuthorized) {
      authorized = notAuthorized = false;
      newCard = false;
      delay(1000);
    }

    digitalWrite(SUCC_LED, LOW);
    digitalWrite(ERR_LED, LOW);

    if (!rfid.PICC_IsNewCardPresent()) {
      return;
    }

    // Verify if the NUID has been readed
    if (!rfid.PICC_ReadCardSerial()) {
      return;
    }

    if (!newCard) {
      Serial.println(F("The NUID tag is:"));
      Serial.print(F("In hex: "));
      Serial.print(getHexString(rfid.uid.uidByte, rfid.uid.size));
      Serial.println();

      sendRequest(getHexString(rfid.uid.uidByte, rfid.uid.size));

      // Halt PICC
      rfid.PICC_HaltA();

      // Stop encryption on PCD
      rfid.PCD_StopCrypto1();
      delay(500);
    }

    newCard = true;
  }
}

String getHexString(byte *buffer, byte bufferSize) {
  String hex;
  for (byte i = 0; i < bufferSize; i++) {
    if (buffer[i] < 0x10) {
      hex += '0';
    }
    hex += String(buffer[i], HEX);
  }
  return hex;
}
