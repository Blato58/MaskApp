package cn.com.heaton.shiningmask.ui.widget;

import android.content.Context;
import android.graphics.Color;
import android.util.AttributeSet;
import android.view.MotionEvent;
import android.view.View;
import android.view.ViewGroup;
import cn.com.heaton.shiningmask.model.bean.Point;
import cn.com.heaton.shiningmask.ui.utils.LogUtil;
import cn.com.heaton.shiningmask.ui.utils.ScreenUtils;
import com.alibaba.fastjson2.internal.asm.Opcodes;
import java.util.ArrayList;
import java.util.LinkedList;
import java.util.List;

/* JADX INFO: loaded from: classes.dex */
public class LedView extends ViewGroup {
    public static final int MODE_ERASER = 2;
    public static final int MODE_NO = 0;
    public static final int MODE_PAINT = 1;
    public static final int ORIENTATION_LANDSCAPE = 1;
    public static final int ORIENTATION_PORTRAIT = 0;
    private static final String TAG = "LedView";
    private int colorB;
    private int colorG;
    private int colorR;
    private int curColor;
    private int curColumn;
    private int curRow;
    private float height;
    private int heightCount;
    int heightSize;
    private boolean isDispatchTouch;
    private LedListener ledListener;
    private Context mContext;
    private int measureHeightMode;
    private int measureWidthMode;
    private int mode;
    int moveMax;
    int moveYMax;
    private int offset;
    private int orientation;
    private int pointAllLength;
    private int pointLength;
    LinkedList<Point> pointList;
    private int pointMargin;
    private int pointYAllLength;
    private RealTimeDataListener realTimeDataListener;
    private int selectedColor1;
    private int selectedColor2;
    private int selectedColor3;
    private int unSelectedColor;
    private float width;
    private int widthCount;
    int widthSize;
    private int xMore;
    private int yMore;

    public interface LedListener {
        void onItemSelect(int i, int i2, int i3, boolean z);
    }

    public interface RealTimeDataListener {
        void onRealTimeData(int i, byte[] bArr);
    }

    public LedView(Context context) {
        super(context);
        this.widthCount = 24;
        this.heightCount = 9;
        this.pointMargin = 1;
        this.unSelectedColor = Color.rgb(1, 255, 255);
        this.selectedColor3 = Color.rgb(82, 227, 255);
        this.selectedColor2 = Color.argb(Opcodes.IF_ACMPNE, 82, 227, 255);
        this.selectedColor1 = Color.argb(89, 82, 227, 255);
        this.isDispatchTouch = true;
        this.curRow = -1;
        this.curColumn = -1;
        this.pointList = new LinkedList<>();
        this.mContext = context;
    }

    public LedView(Context context, AttributeSet attributeSet) {
        super(context, attributeSet);
        this.widthCount = 24;
        this.heightCount = 9;
        this.pointMargin = 1;
        this.unSelectedColor = Color.rgb(1, 255, 255);
        this.selectedColor3 = Color.rgb(82, 227, 255);
        this.selectedColor2 = Color.argb(Opcodes.IF_ACMPNE, 82, 227, 255);
        this.selectedColor1 = Color.argb(89, 82, 227, 255);
        this.isDispatchTouch = true;
        this.curRow = -1;
        this.curColumn = -1;
        this.pointList = new LinkedList<>();
        this.mContext = context;
    }

    public LedView(Context context, AttributeSet attributeSet, int i) {
        super(context, attributeSet, i);
        this.widthCount = 24;
        this.heightCount = 9;
        this.pointMargin = 1;
        this.unSelectedColor = Color.rgb(1, 255, 255);
        this.selectedColor3 = Color.rgb(82, 227, 255);
        this.selectedColor2 = Color.argb(Opcodes.IF_ACMPNE, 82, 227, 255);
        this.selectedColor1 = Color.argb(89, 82, 227, 255);
        this.isDispatchTouch = true;
        this.curRow = -1;
        this.curColumn = -1;
        this.pointList = new LinkedList<>();
        this.mContext = context;
    }

    public void init(int i, int i2) {
        this.height = ScreenUtils.getScreenHeight(this.mContext);
        this.width = ScreenUtils.getScreenWidth(this.mContext);
        this.widthCount = i;
        this.heightCount = i2;
        for (int i3 = 0; i3 < i * i2; i3++) {
            LedItemView ledItemView = new LedItemView(getContext());
            ledItemView.setViewNumber(i3);
            ledItemView.setColumnNumber(i3 / i2);
            ledItemView.setRowNumber(i3 % i2);
            ledItemView.setPaint(this.unSelectedColor);
            ledItemView.postInvalidate();
            addView(ledItemView);
        }
    }

    public void init(int i, int i2, float f) {
        this.widthCount = i;
        this.heightCount = i2;
        for (int i3 = 0; i3 < i * i2; i3++) {
            LedItemView ledItemView = new LedItemView(getContext());
            ledItemView.setViewNumber(i3);
            ledItemView.setColumnNumber(i3 / i2);
            ledItemView.setRowNumber(i3 % i2);
            ledItemView.setPaint(this.unSelectedColor, f);
            ledItemView.postInvalidate();
            addView(ledItemView);
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
        LogUtil.d("height:" + this.height + "  width:" + this.width);
        float f = this.width / this.height;
        LogUtil.d("===screenPortion:" + f);
        double d = f;
        if (d >= 2.1d) {
            this.widthSize = (int) (((double) View.MeasureSpec.getSize(i)) * 0.85d);
        } else if (d >= 1.9d) {
            this.widthSize = View.MeasureSpec.getSize(i) - ScreenUtils.dp2px(this.mContext, 80.0f);
        } else if (d >= 1.6d) {
            this.widthSize = View.MeasureSpec.getSize(i) - ScreenUtils.dp2px(this.mContext, 12.0f);
        } else {
            this.widthSize = View.MeasureSpec.getSize(i - ScreenUtils.dp2px(this.mContext, 11.0f));
        }
        this.heightSize = View.MeasureSpec.getSize(i2);
        this.measureWidthMode = View.MeasureSpec.getMode(i);
        this.measureHeightMode = View.MeasureSpec.getMode(i2);
        LogUtil.d("widthSize:" + this.widthSize + " heightSize:" + this.heightSize);
        int i3 = this.widthSize;
        int i4 = this.widthCount;
        int i5 = i3 % i4;
        this.xMore = i5;
        int i6 = this.heightSize;
        int i7 = this.heightCount;
        this.yMore = i6 % i7;
        int i8 = i3 / i4;
        this.pointAllLength = i8;
        int i9 = i6 / i7;
        this.pointYAllLength = i9;
        this.pointLength = i8 - (this.pointMargin * 2);
        this.offset = i5 / 2;
        this.moveMax = (i8 * i7) - i6;
        this.moveYMax = (i9 * i4) - i3;
        LogUtil.d("xMore:" + this.xMore + " yMore:" + this.yMore);
        int i10 = (this.pointAllLength * this.heightCount) - (this.pointMargin + this.offset);
        this.heightSize = i10;
        setMeasuredDimension(this.widthSize, i10);
    }

    @Override // android.view.ViewGroup, android.view.View
    protected void onLayout(boolean z, int i, int i2, int i3, int i4) {
        LogUtil.d("changed:" + z + " l:" + i + " t:" + i2 + " r:" + i3 + " b" + i4);
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
        float scrollY = getScrollY();
        float y = motionEvent.getY() + scrollY;
        if (action == 2 || action == 0) {
            if (x < 0.0f || this.widthSize < x || y < 0.0f || this.heightSize + scrollY < y) {
                return true;
            }
            int i = this.offset;
            int i2 = this.pointAllLength;
            int i3 = (((int) ((x - i) / i2)) * this.heightCount) + ((int) ((y - i) / i2));
            if (i3 >= 0 && i3 < getChildCount()) {
                LedItemView ledItemView = (LedItemView) getChildAt(i3);
                int i4 = this.mode;
                if (i4 == 1) {
                    ledItemView.setChecked(true);
                    ledItemView.setColor(this.curColor);
                    int i5 = this.curColor;
                    if (i5 != 0) {
                        ledItemView.setPaint(i5);
                    } else {
                        ledItemView.setPaint(this.selectedColor3);
                    }
                    ledItemView.postInvalidate();
                } else {
                    if (i4 != 2 || !ledItemView.isChecked()) {
                        return true;
                    }
                    ledItemView.setChecked(false);
                    ledItemView.setPaint(this.unSelectedColor);
                    ledItemView.postInvalidate();
                }
                if (this.ledListener != null) {
                    if (this.curRow == ledItemView.getRowNumber() && this.curColumn == ledItemView.getColumnNumber()) {
                        return true;
                    }
                    this.curRow = ledItemView.getRowNumber();
                    this.curColumn = ledItemView.getColumnNumber();
                    this.ledListener.onItemSelect(ledItemView.getViewNumber(), ledItemView.getColumnNumber(), ledItemView.getRowNumber(), ledItemView.isChecked());
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

    public void setPaintColor(int i) {
        this.curColor = i;
        this.colorR = Color.red(i);
        this.colorG = Color.green(i);
        this.colorB = Color.blue(i);
    }

    public void clearSelected() {
        for (int i = 0; i < getChildCount(); i++) {
            LedItemView ledItemView = (LedItemView) getChildAt(i);
            ledItemView.setChecked(false);
            ledItemView.setPaint(this.unSelectedColor);
            ledItemView.postInvalidate();
        }
    }

    public byte[] reverse(byte[] bArr) {
        if (bArr == null) {
            return null;
        }
        for (int i = 0; i < bArr.length; i++) {
            LogUtil.d("====data:" + ((int) bArr[i]));
            byte b = bArr[i];
            if (b == 3) {
                bArr[i] = 0;
            } else if (b == 2) {
                bArr[i] = 1;
            } else if (b == 1) {
                bArr[i] = 2;
            } else if (b == 0) {
                bArr[i] = 3;
            }
        }
        return bArr;
    }

    public void setItemSelected(int i, boolean z) {
        if (i < 0 || i >= getChildCount()) {
            return;
        }
        LedItemView ledItemView = (LedItemView) getChildAt(i);
        if (z) {
            ledItemView.setChecked(true);
            ledItemView.setPaint(this.selectedColor1);
            ledItemView.postInvalidate();
        } else {
            ledItemView.setChecked(false);
            ledItemView.setPaint(this.unSelectedColor);
            ledItemView.postInvalidate();
        }
    }

    public byte[] getData() {
        int childCount = getChildCount();
        LogUtil.d("childCount:" + childCount);
        byte[] bArr = new byte[childCount];
        for (int i = 0; i < childCount; i++) {
            LedItemView ledItemView = (LedItemView) getChildAt(i);
            if (ledItemView.isChecked()) {
                LogUtil.d("color:" + ledItemView.getColor());
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
            LedItemView ledItemView = (LedItemView) getChildAt(i);
            LogUtil.d("color:" + ledItemView.getColor());
            arrayList.add(Integer.valueOf(ledItemView.getColor()));
        }
        return arrayList;
    }

    public void setData(List<Integer> list, byte[] bArr) {
        int childCount;
        if (list == null || bArr == null || list.size() != (childCount = getChildCount())) {
            return;
        }
        for (int i = 0; i < childCount; i++) {
            LedItemView ledItemView = (LedItemView) getChildAt(i);
            if (bArr[i] == 1) {
                ledItemView.setChecked(true);
                ledItemView.setColor(list.get(i).intValue());
                ledItemView.setPaint(list.get(i).intValue());
            } else {
                ledItemView.setChecked(false);
                ledItemView.setPaint(this.unSelectedColor);
            }
            ledItemView.postInvalidate();
        }
    }

    public void setData(byte[] bArr, float f) {
        int childCount;
        if (bArr != null && bArr.length == (childCount = getChildCount())) {
            for (int i = 0; i < childCount; i++) {
                LedItemView ledItemView = (LedItemView) getChildAt(i);
                if (bArr[i] == 1) {
                    ledItemView.setChecked(true);
                    ledItemView.setPaint(this.selectedColor1, f);
                } else {
                    ledItemView.setChecked(false);
                    ledItemView.setPaint(this.unSelectedColor, f);
                }
                ledItemView.postInvalidate();
            }
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

    public int getMoveMax() {
        return this.moveMax;
    }

    public void setUnSelectedColor(int i) {
        this.unSelectedColor = i;
    }

    public void setSelectedColor(int i) {
        this.selectedColor1 = i;
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

    public void setDispatchTouch(boolean z) {
        this.isDispatchTouch = z;
    }

    public RealTimeDataListener getRealTimeDataListener() {
        return this.realTimeDataListener;
    }

    public void setRealTimeDataListener(RealTimeDataListener realTimeDataListener) {
        this.realTimeDataListener = realTimeDataListener;
    }
}