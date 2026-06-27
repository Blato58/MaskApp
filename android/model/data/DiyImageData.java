package cn.com.heaton.shiningmask.model.data;

/* JADX INFO: loaded from: classes.dex */
public class DiyImageData {
    private int curIndex;
    private int dataLength;
    private byte[] sendData;

    public int getCurIndex() {
        return this.curIndex;
    }

    public void setCurIndex(int i) {
        this.curIndex = i;
    }

    public byte[] getSendData() {
        return this.sendData;
    }

    public void setSendData(byte[] bArr) {
        this.sendData = bArr;
    }

    public int getDataLength() {
        return this.dataLength;
    }

    public void setDataLength(int i) {
        this.dataLength = i;
    }

    public String toString() {
        return "DiyImageData{curIndex=" + this.curIndex + ", dataLength=" + this.dataLength + '}';
    }
}