package cn.com.heaton.shiningmask.model.bean;

import java.util.Arrays;

/* JADX INFO: loaded from: classes.dex */
public class TextData {
    private int color;
    private int colorB;
    private int colorG;
    private int colorR;
    private String content;
    private byte[] data;
    private int type;
    private int widthCount;

    public String getContent() {
        return this.content;
    }

    public void setContent(String str) {
        this.content = str;
    }

    public int getType() {
        return this.type;
    }

    public void setType(int i) {
        this.type = i;
    }

    public int getWidthCount() {
        return this.widthCount;
    }

    public void setWidthCount(int i) {
        this.widthCount = i;
    }

    public byte[] getData() {
        return this.data;
    }

    public void setData(byte[] bArr) {
        this.data = bArr;
    }

    public int getColor() {
        return this.color;
    }

    public void setColor(int i) {
        this.color = i;
    }

    public int getColorR() {
        return this.colorR;
    }

    public void setColorR(int i) {
        this.colorR = i;
    }

    public int getColorG() {
        return this.colorG;
    }

    public void setColorG(int i) {
        this.colorG = i;
    }

    public int getColorB() {
        return this.colorB;
    }

    public void setColorB(int i) {
        this.colorB = i;
    }

    public String toString() {
        return "TextData{content='" + this.content + "', type=" + this.type + ", widthCount=" + this.widthCount + ", data=" + Arrays.toString(this.data) + ", color=" + this.color + ", colorR=" + this.colorR + ", colorG=" + this.colorG + ", colorB=" + this.colorB + '}';
    }
}