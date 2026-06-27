package cn.com.heaton.shiningmask.base;

import android.graphics.Color;

/* JADX INFO: loaded from: classes.dex */
public class DataManager {
    private static DataManager instance;
    private boolean isSnyImage;
    public byte[] lastRhyhmData;
    private boolean microphoneEnable;
    public boolean isShowGPSDialog = false;
    public boolean isRhythm = false;
    public int curSelectRhythmMode = 0;
    public int curSelectRhythmMode1 = 0;
    public boolean isReConnect = true;
    private int curLeftRightMoveType = 0;
    private int curTopBottomMoveType = 0;
    private int curOtaVersion = 0;
    private String curText = "WOW";
    private boolean textColorEnable = false;
    private boolean textColorBgEnable = false;
    private int textColorMode = 0;
    private int textColorBgMode = 0;
    private float textColorProgress = 3.0f;
    private float textColorBgProgress = -1.0f;
    private int curSpeed = 50;
    private int curLight = 50;
    private int curTextMode = 0;
    private int textColor = -788529156;
    private int textBgColor = Color.rgb(127, 127, 127);
    private float textColorAngle = 0.0f;
    private float textColorBgAngle = 0.0f;

    public static DataManager getInstance() {
        if (instance == null) {
            synchronized (DataManager.class) {
                if (instance == null) {
                    instance = new DataManager();
                }
            }
        }
        return instance;
    }

    public float getTextColorAngle() {
        return this.textColorAngle;
    }

    public void setTextColorAngle(float f) {
        this.textColorAngle = f;
    }

    public int getTextBgColor() {
        return this.textBgColor;
    }

    public void setTextBgColor(int i) {
        this.textBgColor = i;
    }

    public float getTextColorBgAngle() {
        return this.textColorBgAngle;
    }

    public void setTextColorBgAngle(float f) {
        this.textColorBgAngle = f;
    }

    public int getTextColor() {
        return this.textColor;
    }

    public void setTextColor(int i) {
        this.textColor = i;
    }

    public boolean isRhythm() {
        return this.isRhythm;
    }

    public void setRhythm(boolean z) {
        this.isRhythm = z;
    }

    public int getCurSelectRhythmMode() {
        return this.curSelectRhythmMode;
    }

    public void setCurSelectRhythmMode(int i) {
        this.curSelectRhythmMode = i;
    }

    public int getCurSelectRhythmMode1() {
        return this.curSelectRhythmMode1;
    }

    public void setCurSelectRhythmMode1(int i) {
        this.curSelectRhythmMode1 = i;
    }

    public byte[] getLastRhyhmData() {
        return this.lastRhyhmData;
    }

    public void setLastRhyhmData(byte[] bArr) {
        this.lastRhyhmData = bArr;
    }

    public int getCurSpeed() {
        return this.curSpeed;
    }

    public void setCurSpeed(int i) {
        this.curSpeed = i;
    }

    public int getCurLight() {
        return this.curLight;
    }

    public void setCurLight(int i) {
        this.curLight = i;
    }

    public int getCurTextMode() {
        return this.curTextMode;
    }

    public void setCurTextMode(int i) {
        this.curTextMode = i;
    }

    public boolean isReConnect() {
        return this.isReConnect;
    }

    public void setReConnect(boolean z) {
        this.isReConnect = z;
    }

    public boolean isShowGPSDialog() {
        return this.isShowGPSDialog;
    }

    public void setShowGPSDialog(boolean z) {
        this.isShowGPSDialog = z;
    }

    public int getCurLeftRightMoveType() {
        return this.curLeftRightMoveType;
    }

    public void setCurLeftRightMoveType(int i) {
        this.curLeftRightMoveType = i;
    }

    public int getCurTopBottomMoveType() {
        return this.curTopBottomMoveType;
    }

    public void setCurTopBottomMoveType(int i) {
        this.curTopBottomMoveType = i;
    }

    public int getCurOtaVersion() {
        return this.curOtaVersion;
    }

    public void setCurOtaVersion(int i) {
        this.curOtaVersion = i;
    }

    public String getCurText() {
        return this.curText;
    }

    public void setCurText(String str) {
        this.curText = str;
    }

    public boolean isTextColorEnable() {
        return this.textColorEnable;
    }

    public void setTextColorEnable(boolean z) {
        this.textColorEnable = z;
    }

    public boolean isTextColorBgEnable() {
        return this.textColorBgEnable;
    }

    public void setTextColorBgEnable(boolean z) {
        this.textColorBgEnable = z;
    }

    public int getTextColorMode() {
        return this.textColorMode;
    }

    public void setTextColorMode(int i) {
        this.textColorMode = i;
    }

    public int getTextColorBgMode() {
        return this.textColorBgMode;
    }

    public void setTextColorBgMode(int i) {
        this.textColorBgMode = i;
    }

    public float getTextColorProgress() {
        return this.textColorProgress;
    }

    public void setTextColorProgress(float f) {
        this.textColorProgress = f;
    }

    public float getTextColorBgProgress() {
        return this.textColorBgProgress;
    }

    public void setTextColorBgProgress(float f) {
        this.textColorBgProgress = f;
    }

    public boolean isMicrophoneEnable() {
        return this.microphoneEnable;
    }

    public void setMicrophoneEnable(boolean z) {
        this.microphoneEnable = z;
    }

    public boolean isSnyImage() {
        return this.isSnyImage;
    }

    public void setSnyImage(boolean z) {
        this.isSnyImage = z;
    }
}