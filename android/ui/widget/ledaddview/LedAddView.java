package cn.com.heaton.shiningmask.ui.widget.ledaddview;

import android.content.Context;
import android.graphics.Color;
import android.util.AttributeSet;
import android.view.View;
import android.view.ViewConfiguration;
import android.view.ViewGroup;
import cn.com.heaton.shiningmask.ui.utils.DensityUtil;
import cn.com.heaton.shiningmask.ui.utils.LogUtil;
import cn.com.heaton.shiningmask.ui.widget.LedItemView1;
import kotlin.UByte;

/* JADX INFO: loaded from: classes.dex */
public class LedAddView extends ViewGroup {
    public static final int MODE_ERASER = 2;
    public static final int MODE_NO = 0;
    public static final int MODE_PAINT = 1;
    public static final int ORIENTATION_LANDSCAPE = 1;
    public static final int ORIENTATION_PORTRAIT = 0;
    private static final String TAG = "LedView";
    private int heightCount;
    int heightSize;
    private boolean isDispatchTouch;
    private boolean isOpen;
    private boolean isValidToggle;
    private int lastX;
    private int lastY;
    private LedListener ledListener;
    private int mLastX;
    private int mode;
    int moveMax;
    private int offset;
    private int orientation;
    private int pointAllLength;
    private int pointLength;
    private int pointMargin;
    private RealTimeDataListener realTimeDataListener;
    private int selectedColor;
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

    public LedAddView(Context context) {
        super(context);
        this.widthCount = 128;
        this.heightCount = 16;
        this.pointMargin = 0;
        this.unSelectedColor = 0;
        this.selectedColor = Color.rgb(18, 255, 1);
        this.isDispatchTouch = true;
    }

    public LedAddView(Context context, AttributeSet attributeSet) {
        super(context, attributeSet);
        this.widthCount = 128;
        this.heightCount = 16;
        this.pointMargin = 0;
        this.unSelectedColor = 0;
        this.selectedColor = Color.rgb(18, 255, 1);
        this.isDispatchTouch = true;
        ViewConfiguration.get(context);
    }

    public LedAddView(Context context, AttributeSet attributeSet, int i) {
        super(context, attributeSet, i);
        this.widthCount = 128;
        this.heightCount = 16;
        this.pointMargin = 0;
        this.unSelectedColor = 0;
        this.selectedColor = Color.rgb(18, 255, 1);
        this.isDispatchTouch = true;
    }

    public void init(int i, int i2) {
        LogUtil.d("widthCount:" + i + " heightCount:" + i2);
        this.widthCount = i;
        this.heightCount = i2;
        for (int i3 = 0; i3 < i * i2; i3++) {
            LedItemView1 ledItemView1 = new LedItemView1(getContext());
            ledItemView1.setViewNumber(i3);
            ledItemView1.setColumnNumber(i3 / i2);
            ledItemView1.setRowNumber(i3 % i2);
            ledItemView1.setPaint(this.unSelectedColor);
            ledItemView1.postInvalidate();
            addView(ledItemView1);
        }
    }

    public void init(int i, int i2, float f) {
        this.widthCount = i;
        this.heightCount = i2;
        for (int i3 = 0; i3 < i * i2; i3++) {
            LedItemView1 ledItemView1 = new LedItemView1(getContext());
            ledItemView1.setViewNumber(i3);
            ledItemView1.setColumnNumber(i3 / i2);
            ledItemView1.setRowNumber(i3 % i2);
            ledItemView1.setPaint(this.unSelectedColor, f);
            ledItemView1.postInvalidate();
            addView(ledItemView1);
        }
    }

    @Override // android.view.View
    protected void onMeasure(int i, int i2) {
        measureChildren(i, i2);
        this.widthSize = View.MeasureSpec.getSize(i);
        int iDp2px = (int) DensityUtil.dp2px(getContext(), 110.0f);
        int size = View.MeasureSpec.getSize(i);
        this.widthSize = size;
        int i3 = this.heightCount;
        int i4 = iDp2px % i3;
        this.xMore = i4;
        int i5 = iDp2px / i3;
        this.pointAllLength = i5;
        this.pointLength = i5 - (this.pointMargin * 2);
        this.offset = i4 / 2;
        this.moveMax = (i5 * this.widthCount) - size;
        LogUtil.d("moveMax:" + this.moveMax + " offset:" + this.offset + " pointLength:" + this.pointLength + " pointAllLength:" + this.pointAllLength + "widthSize:" + this.widthSize);
        int iDp2px2 = (int) ((this.pointAllLength * this.widthCount) + DensityUtil.dp2px(getContext(), 0.0f));
        LogUtil.d("当前的宽度：pointAllLength:" + this.pointAllLength + "   width:" + iDp2px2 + "  xMore:" + (this.xMore * this.widthCount));
        setMeasuredDimension(iDp2px2, iDp2px);
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

    public byte[] getData(int i) {
        int childCount = getChildCount();
        LogUtil.d("childCount:" + childCount);
        byte[] bArr = new byte[childCount];
        int i2 = this.orientation;
        int i3 = 0;
        if (i2 == 0) {
            if (i == 0) {
                while (i3 < childCount) {
                    if (((LedItemView1) getChildAt(i3)).isChecked()) {
                        bArr[i3] = 1;
                    }
                    i3++;
                }
            } else if (i == 1) {
                LogUtil.d("横屏数据childCount:" + childCount);
                while (i3 < childCount) {
                    if (((LedItemView1) getChildAt(i3)).isChecked()) {
                        int i4 = this.heightCount;
                        int i5 = this.widthCount;
                        bArr[((i3 % i4) * i5) + ((i5 - (i3 / i4)) - 1)] = 1;
                    }
                    i3++;
                }
            }
        } else if (i2 == 1) {
            if (i == 0) {
                while (i3 < childCount) {
                    if (((LedItemView1) getChildAt(i3)).isChecked()) {
                        int i6 = this.heightCount;
                        bArr[(((i6 - (i3 % i6)) - 1) * this.widthCount) + (i3 / i6)] = 1;
                    }
                    i3++;
                }
            } else if (i == 1) {
                while (i3 < childCount) {
                    if (((LedItemView1) getChildAt(i3)).isChecked()) {
                        bArr[i3] = 1;
                    }
                    i3++;
                }
            }
        }
        return bArr;
    }

    public void setData(byte[] bArr) {
        int childCount;
        if (bArr != null && bArr.length == (childCount = getChildCount())) {
            for (int i = 0; i < childCount; i++) {
                LedItemView1 ledItemView1 = (LedItemView1) getChildAt(i);
                if (bArr[i] == 1) {
                    ledItemView1.setChecked(true);
                    ledItemView1.setPaint(this.selectedColor);
                } else {
                    ledItemView1.setChecked(false);
                    ledItemView1.setPaint(this.unSelectedColor);
                }
                ledItemView1.postInvalidate();
            }
        }
    }

    public void setData(byte[] bArr, float f) {
        int childCount;
        if (bArr != null && bArr.length == (childCount = getChildCount())) {
            for (int i = 0; i < childCount; i++) {
                LedItemView1 ledItemView1 = (LedItemView1) getChildAt(i);
                if (bArr[i] == 1) {
                    ledItemView1.setChecked(true);
                    ledItemView1.setPaint(this.selectedColor, f);
                } else {
                    ledItemView1.setChecked(false);
                    ledItemView1.setPaint(this.unSelectedColor, f);
                }
                ledItemView1.postInvalidate();
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
        this.selectedColor = i;
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

    public RealTimeDataListener getRealTimeDataListener() {
        return this.realTimeDataListener;
    }

    public void setRealTimeDataListener(RealTimeDataListener realTimeDataListener) {
        this.realTimeDataListener = realTimeDataListener;
    }

    public void setTextData(byte[] bArr) {
        LogUtil.d("设置文本数据：" + bArr.length);
        int length = bArr.length / 2;
        int[] iArr = new int[length];
        for (int i = 0; i < bArr.length / 2; i++) {
            int i2 = i * 2;
            iArr[i] = ((bArr[i2] & UByte.MAX_VALUE) * 256) + (bArr[i2 + 1] & UByte.MAX_VALUE);
        }
        for (int i3 = 0; i3 < this.widthCount; i3++) {
            if (i3 < length) {
                for (int i4 = 0; i4 < 16; i4++) {
                    int i5 = (iArr[i3] >> i4) & 1;
                    LedItemView1 ledItemView1 = getLedItemView1(i3, i4);
                    if (ledItemView1 == null) {
                        return;
                    }
                    if (i5 == 1) {
                        ledItemView1.setChecked(true);
                        ledItemView1.setPaint(this.selectedColor);
                        LogUtil.d(i4 + "行" + i3 + "列light:" + i5);
                    } else {
                        ledItemView1.setChecked(false);
                        ledItemView1.setPaint(this.unSelectedColor);
                    }
                    ledItemView1.postInvalidate();
                }
            }
        }
    }

    private LedItemView1 getLedItemView1(int i, int i2) {
        LogUtil.d("行：" + i2 + " 列：" + i);
        return (LedItemView1) getChildAt((i * 16) + (16 - (i2 + 1)));
    }
}