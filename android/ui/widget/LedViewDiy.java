package cn.com.heaton.shiningmask.ui.widget;

import android.content.Context;
import android.graphics.Color;
import android.util.AttributeSet;
import android.view.MotionEvent;
import android.view.View;
import android.view.ViewGroup;
import cn.com.heaton.shiningmask.model.bean.Point;
import cn.com.heaton.shiningmask.ui.utils.LogUtil;
import java.util.ArrayList;
import java.util.LinkedList;
import java.util.List;

/* JADX INFO: loaded from: classes.dex */
public class LedViewDiy extends ViewGroup {
    public static final int MODE_ERASER = 2;
    public static final int MODE_NO = 0;
    public static final int MODE_PAINT = 1;
    private static final String TAG = "LedView";
    private int colorB;
    private int colorG;
    private int colorR;
    private int curColor;
    private int curColumn;
    private int curRow;
    private int heightCount;
    int heightSize;
    private boolean isDispatchTouch;
    private LedListener ledListener;
    private int mode;
    int moveMax;
    private int offset;
    private int orientation;
    private int pointAllLength;
    private int pointLength;
    private LinkedList<Point> pointList;
    private int pointMargin;
    private RealTimeDataListener realTimeDataListener;
    private int selectedColor1;
    private int selectedColor3;
    private int unSelectedColor;
    private int widthCount;
    int widthSize;
    private int xMore;

    public interface LedListener {
        void onItemSelect(int i, int i2, int i3, boolean z);
    }

    public interface RealTimeDataListener {
        void onRealTimeData(int i, byte[] bArr);
    }

    public LedViewDiy(Context context) {
        super(context);
        this.widthCount = 128;
        this.heightCount = 16;
        this.pointMargin = 0;
        this.unSelectedColor = Color.rgb(1, 255, 255);
        this.selectedColor3 = Color.rgb(82, 227, 255);
        this.selectedColor1 = Color.argb(89, 82, 227, 255);
        this.curRow = -1;
        this.curColumn = -1;
        this.pointList = new LinkedList<>();
        this.isDispatchTouch = true;
    }

    public LedViewDiy(Context context, AttributeSet attributeSet) {
        super(context, attributeSet);
        this.widthCount = 128;
        this.heightCount = 16;
        this.pointMargin = 0;
        this.unSelectedColor = Color.rgb(1, 255, 255);
        this.selectedColor3 = Color.rgb(82, 227, 255);
        this.selectedColor1 = Color.argb(89, 82, 227, 255);
        this.curRow = -1;
        this.curColumn = -1;
        this.pointList = new LinkedList<>();
        this.isDispatchTouch = true;
    }

    public LedViewDiy(Context context, AttributeSet attributeSet, int i) {
        super(context, attributeSet, i);
        this.widthCount = 128;
        this.heightCount = 16;
        this.pointMargin = 0;
        this.unSelectedColor = Color.rgb(1, 255, 255);
        this.selectedColor3 = Color.rgb(82, 227, 255);
        this.selectedColor1 = Color.argb(89, 82, 227, 255);
        this.curRow = -1;
        this.curColumn = -1;
        this.pointList = new LinkedList<>();
        this.isDispatchTouch = true;
    }

    public void init(int i, int i2, float f) {
        this.widthCount = i;
        this.heightCount = i2;
        for (int i3 = 0; i3 < i * i2; i3++) {
            DiyLedItemView diyLedItemView = new DiyLedItemView(getContext());
            diyLedItemView.setViewNumber(i3);
            diyLedItemView.setColumnNumber(i3 / i2);
            diyLedItemView.setRowNumber(i3 % i2);
            diyLedItemView.setPaint(this.unSelectedColor, f);
            diyLedItemView.postInvalidate();
            addView(diyLedItemView);
        }
    }

    public void removeAllChildView() {
        int childCount = getChildCount();
        if (childCount > 0) {
            for (int i = 0; i < childCount; i++) {
                removeViewAt(0);
            }
        }
    }

    @Override // android.view.View
    protected void onMeasure(int i, int i2) {
        measureChildren(i, i2);
        this.widthSize = View.MeasureSpec.getSize(i);
        int size = View.MeasureSpec.getSize(i2);
        this.heightSize = size;
        int i3 = this.heightCount;
        int i4 = size % i3;
        this.xMore = i4;
        int i5 = size / i3;
        this.pointAllLength = i5;
        this.pointLength = i5 - (this.pointMargin * 2);
        this.offset = i4 / 2;
        int i6 = i5 * this.widthCount;
        int i7 = this.widthSize;
        this.moveMax = i6 - i7;
        setMeasuredDimension(i7, size);
    }

    public int getPointAllLength() {
        return this.pointAllLength;
    }

    @Override // android.view.ViewGroup, android.view.View
    protected void onLayout(boolean z, int i, int i2, int i3, int i4) {
        int childCount = getChildCount();
        for (int i5 = 0; i5 < childCount; i5++) {
            View childAt = getChildAt(i5);
            int i6 = this.heightCount;
            int i7 = this.pointAllLength;
            int i8 = this.pointMargin;
            int i9 = ((i5 / i6) * i7) + i8 + this.offset;
            int i10 = ((i5 % i6) * i7) + i8;
            int i11 = this.pointLength;
            childAt.layout(i9, i10, i9 + i11, i11 + i10);
        }
    }

    @Override // android.view.ViewGroup, android.view.View
    public boolean dispatchTouchEvent(MotionEvent motionEvent) {
        int action = motionEvent.getAction();
        float x = motionEvent.getX();
        float y = motionEvent.getY() + getScrollY();
        if (action == 2 || action == 0) {
            if (x < 0.0f || this.widthSize < x || y < 0.0f || y > this.heightSize) {
                return true;
            }
            int i = this.offset;
            int i2 = this.pointAllLength;
            int i3 = (int) ((y - i) / i2);
            int i4 = (int) ((x - i) / i2);
            if (i3 >= 12 || i4 >= 36) {
                return true;
            }
            int i5 = (i4 * this.heightCount) + i3;
            if (i5 >= 0 && i5 < getChildCount()) {
                DiyLedItemView diyLedItemView = (DiyLedItemView) getChildAt(i5);
                int i6 = this.mode;
                if (i6 == 1) {
                    diyLedItemView.setChecked(true);
                    diyLedItemView.setColor(this.curColor);
                    int i7 = this.curColor;
                    if (i7 != 0) {
                        diyLedItemView.setPaint(i7);
                    } else {
                        diyLedItemView.setPaint(this.selectedColor3);
                    }
                    diyLedItemView.postInvalidate();
                } else {
                    if (i6 != 2 || !diyLedItemView.isChecked()) {
                        return true;
                    }
                    diyLedItemView.setChecked(false);
                    diyLedItemView.setColor(0);
                    diyLedItemView.setPaint(this.unSelectedColor);
                    diyLedItemView.postInvalidate();
                }
                if (this.ledListener != null) {
                    if (this.curRow == diyLedItemView.getRowNumber() && this.curColumn == diyLedItemView.getColumnNumber()) {
                        return true;
                    }
                    this.curRow = diyLedItemView.getRowNumber();
                    this.curColumn = diyLedItemView.getColumnNumber();
                    this.ledListener.onItemSelect(diyLedItemView.getViewNumber(), diyLedItemView.getColumnNumber(), diyLedItemView.getRowNumber(), diyLedItemView.isChecked());
                }
            }
        } else if (action == 1) {
            LinkedList<Point> linkedList = this.pointList;
            if (linkedList != null) {
                linkedList.clear();
            }
            this.curRow = -1;
            this.curColumn = -1;
        }
        return this.isDispatchTouch;
    }

    public void clearSelected() {
        for (int i = 0; i < getChildCount(); i++) {
            DiyLedItemView diyLedItemView = (DiyLedItemView) getChildAt(i);
            diyLedItemView.setChecked(false);
            diyLedItemView.setPaint(this.unSelectedColor);
            diyLedItemView.setColor(0);
            diyLedItemView.postInvalidate();
        }
    }

    public int getWidthCount() {
        return this.widthCount;
    }

    public int getHeightCount() {
        return this.heightCount;
    }

    public void setPointMargin(int i) {
        this.pointMargin = i;
    }

    public void setUnSelectedColor(int i) {
        this.unSelectedColor = i;
    }

    public void setMode(int i) {
        this.mode = i;
    }

    public int getOrientation() {
        return this.orientation;
    }

    public void setOrientation(int i) {
        this.orientation = i;
    }

    public LedListener getLedListener() {
        return this.ledListener;
    }

    public void setLedListener(LedListener ledListener) {
        this.ledListener = ledListener;
    }

    public void setDispatchTouch(boolean z) {
        this.isDispatchTouch = z;
    }

    public RealTimeDataListener getRealTimeDataListener() {
        return this.realTimeDataListener;
    }

    public void setRealTimeDataListener(RealTimeDataListener realTimeDataListener) {
        this.realTimeDataListener = realTimeDataListener;
    }

    public void setPaintColor(int i) {
        this.curColor = i;
        this.colorR = Color.red(i);
        this.colorG = Color.green(i);
        this.colorB = Color.blue(i);
    }

    public byte[] getData() {
        int childCount = getChildCount();
        LogUtil.d("childCount:" + childCount);
        byte[] bArr = new byte[childCount];
        for (int i = 0; i < childCount; i++) {
            DiyLedItemView diyLedItemView = (DiyLedItemView) getChildAt(i);
            if (diyLedItemView.isChecked()) {
                LogUtil.d("color:" + i + "  " + diyLedItemView.getColor());
                bArr[i] = 1;
            } else {
                bArr[i] = 0;
            }
        }
        return bArr;
    }

    public ArrayList<Integer> getDataColor() {
        int childCount = getChildCount();
        LogUtil.d("childCount:" + childCount);
        ArrayList<Integer> arrayList = new ArrayList<>();
        for (int i = 0; i < childCount; i++) {
            DiyLedItemView diyLedItemView = (DiyLedItemView) getChildAt(i);
            LogUtil.d("color:" + i + " " + diyLedItemView.getColor());
            arrayList.add(Integer.valueOf(diyLedItemView.getColor()));
        }
        return arrayList;
    }

    public void setData(List<Integer> list, byte[] bArr) {
        int childCount;
        if (list == null || bArr == null || list.size() != (childCount = getChildCount())) {
            return;
        }
        for (int i = 0; i < childCount; i++) {
            DiyLedItemView diyLedItemView = (DiyLedItemView) getChildAt(i);
            LogUtil.d("data[i]:" + ((int) bArr[i]));
            if (bArr[i] == 1) {
                diyLedItemView.setChecked(true);
                diyLedItemView.setColor(list.get(i).intValue());
                diyLedItemView.setPaint(list.get(i).intValue());
            } else {
                diyLedItemView.setChecked(false);
                diyLedItemView.setPaint(this.unSelectedColor);
            }
            diyLedItemView.postInvalidate();
        }
    }

    public void setData(byte[] bArr, float f) {
        int childCount;
        if (bArr != null && bArr.length == (childCount = getChildCount())) {
            for (int i = 0; i < childCount; i++) {
                DiyLedItemView diyLedItemView = (DiyLedItemView) getChildAt(i);
                if (bArr[i] == 1) {
                    diyLedItemView.setChecked(true);
                    diyLedItemView.setPaint(this.selectedColor1, f);
                } else {
                    diyLedItemView.setChecked(false);
                    diyLedItemView.setPaint(this.unSelectedColor, f);
                }
                diyLedItemView.postInvalidate();
            }
        }
    }

    public void getRealTime(int i, int i2, RealTimeDataListener realTimeDataListener) {
        LogUtil.d("行：" + i + " 列：" + i2);
        byte[] bArr = new byte[20];
        LogUtil.d("当前颜色：r:" + this.colorR + " g:" + this.colorG + " b:" + this.colorB + "  cur:" + this.curColor);
        bArr[0] = 5;
        if (this.mode == 2) {
            bArr[1] = 0;
            bArr[2] = 0;
            bArr[3] = 0;
        } else {
            bArr[1] = (byte) Color.red(this.curColor);
            bArr[2] = (byte) Color.green(this.curColor);
            bArr[3] = (byte) Color.blue(this.curColor);
        }
        Point point = new Point((byte) i2, (byte) i);
        if (this.pointList.size() >= 8) {
            this.pointList.remove(0);
        }
        this.pointList.add(point);
        LogUtil.d("pointList:" + this.pointList.size());
        bArr[0] = (byte) ((this.pointList.size() * 2) + 3);
        ArrayList arrayList = new ArrayList();
        for (int i3 = 0; i3 < this.pointList.size(); i3++) {
            LogUtil.d("point:" + this.pointList.get(i3).toString());
            Point point2 = this.pointList.get(i3);
            arrayList.add(Byte.valueOf(point2.getRow()));
            arrayList.add(Byte.valueOf(point2.getColumn()));
        }
        int size = arrayList.size();
        byte[] bArr2 = new byte[size];
        for (int i4 = 0; i4 < arrayList.size(); i4++) {
            bArr2[i4] = ((Byte) arrayList.get(i4)).byteValue();
        }
        System.arraycopy(bArr2, 0, bArr, 4, size);
        if (realTimeDataListener != null) {
            realTimeDataListener.onRealTimeData(1236, bArr);
        }
    }
}