package cn.com.heaton.shiningmask.model.bean;

/* JADX INFO: loaded from: classes.dex */
public class Point {
    private byte column;
    private byte row;

    public Point(byte b, byte b2) {
        this.row = b;
        this.column = b2;
    }

    public byte getRow() {
        return this.row;
    }

    public void setRow(byte b) {
        this.row = b;
    }

    public byte getColumn() {
        return this.column;
    }

    public void setColumn(byte b) {
        this.column = b;
    }

    public String toString() {
        return "Point{row=" + ((int) this.row) + ", column=" + ((int) this.column) + '}';
    }
}