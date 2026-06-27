package cn.com.heaton.shiningmask.model.data;

import java.util.List;

/* JADX INFO: loaded from: classes.dex */
public class SendDiyData {
    private int curIndex;
    List<DiyImageData> dataList;

    public SendDiyData(int i, List<DiyImageData> list) {
        this.curIndex = i;
        this.dataList = list;
    }

    public int getCurIndex() {
        return this.curIndex;
    }

    public void setCurIndex(int i) {
        this.curIndex = i;
    }

    public List<DiyImageData> getDataList() {
        return this.dataList;
    }

    public void setDataList(List<DiyImageData> list) {
        this.dataList = list;
    }

    public String toString() {
        return "SendDiyData{curIndex=" + this.curIndex + ", dataList=" + this.dataList + '}';
    }
}