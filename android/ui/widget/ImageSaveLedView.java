package cn.com.heaton.shiningmask.ui.widget;

import android.content.Context;
import android.graphics.Color;
import android.os.Handler;
import android.os.Message;
import android.util.AttributeSet;
import android.view.View;
import android.view.ViewGroup;
import cn.com.heaton.shiningmask.ui.utils.LogUtil;
import com.alibaba.fastjson2.internal.asm.Opcodes;
import java.util.LinkedList;
import java.util.List;
import java.util.Timer;
import java.util.TimerTask;
import kotlin.UByte;

/* JADX INFO: loaded from: classes.dex */
public class ImageSaveLedView extends ViewGroup {
    private static final String TAG = "LedView";
    Handler handler;
    private int heightCount;
    int heightSize;
    private int[] imageData;
    private LinkedList<Integer> leftMoveList;
    int moveMax;
    int moveYMax;
    MyTimerTask1 myTimerTask1;
    MyTimerTask2 myTimerTask2;
    MyTimerTask3 myTimerTask3;
    MyTimerTask4 myTimerTask4;
    private int offset;
    private int orientation;
    private int pointAllLength;
    private int pointLength;
    private int pointMargin;
    private int pointYAllLength;
    private LinkedList<Integer> rightMoveList;
    private int selectedColor1;
    private int selectedColor2;
    private int selectedColor3;
    private int selectedColorGray1;
    private int selectedColorGray2;
    private int selectedColorGray3;
    private byte[] textData;
    private int timeCount;
    Timer timer;
    LinkedList<Integer> topMoveLeftList;
    LinkedList<Integer> topMoveRightList;
    private int unSelectedColor;
    private boolean up;
    private int widthCount;
    int widthSize;
    private int xMore;
    private int yMore;

    public ImageSaveLedView(Context context) {
        super(context);
        this.widthCount = 5;
        this.heightCount = 10;
        this.pointMargin = 1;
        this.selectedColor3 = Color.rgb(82, 227, 255);
        this.selectedColor2 = Color.argb(Opcodes.IF_ACMPNE, 82, 227, 255);
        this.selectedColor1 = Color.argb(89, 82, 227, 255);
        this.selectedColorGray1 = Color.argb(89, 42, 122, 142);
        this.selectedColorGray2 = Color.argb(Opcodes.IF_ACMPNE, 42, 122, 142);
        this.selectedColorGray3 = Color.rgb(42, 122, 142);
        this.unSelectedColor = -1;
        this.timeCount = 0;
        this.leftMoveList = new LinkedList<>();
        this.rightMoveList = new LinkedList<>();
        this.topMoveRightList = new LinkedList<>();
        this.topMoveLeftList = new LinkedList<>();
        this.handler = new Handler() { // from class: cn.com.heaton.shiningmask.ui.widget.ImageSaveLedView.1
            @Override // android.os.Handler
            public void handleMessage(Message message) {
                if (message.what == 1) {
                    ImageSaveLedView.this.topAnimRight();
                } else if (message.what == 2) {
                    ImageSaveLedView.this.textLeftMove();
                } else if (message.what == 3) {
                    ImageSaveLedView.this.topAnimLeft();
                } else if (message.what == 4) {
                    ImageSaveLedView.this.textRightMove();
                }
                super.handleMessage(message);
            }
        };
    }

    public ImageSaveLedView(Context context, AttributeSet attributeSet) {
        super(context, attributeSet);
        this.widthCount = 5;
        this.heightCount = 10;
        this.pointMargin = 1;
        this.selectedColor3 = Color.rgb(82, 227, 255);
        this.selectedColor2 = Color.argb(Opcodes.IF_ACMPNE, 82, 227, 255);
        this.selectedColor1 = Color.argb(89, 82, 227, 255);
        this.selectedColorGray1 = Color.argb(89, 42, 122, 142);
        this.selectedColorGray2 = Color.argb(Opcodes.IF_ACMPNE, 42, 122, 142);
        this.selectedColorGray3 = Color.rgb(42, 122, 142);
        this.unSelectedColor = -1;
        this.timeCount = 0;
        this.leftMoveList = new LinkedList<>();
        this.rightMoveList = new LinkedList<>();
        this.topMoveRightList = new LinkedList<>();
        this.topMoveLeftList = new LinkedList<>();
        this.handler = new Handler() { // from class: cn.com.heaton.shiningmask.ui.widget.ImageSaveLedView.1
            @Override // android.os.Handler
            public void handleMessage(Message message) {
                if (message.what == 1) {
                    ImageSaveLedView.this.topAnimRight();
                } else if (message.what == 2) {
                    ImageSaveLedView.this.textLeftMove();
                } else if (message.what == 3) {
                    ImageSaveLedView.this.topAnimLeft();
                } else if (message.what == 4) {
                    ImageSaveLedView.this.textRightMove();
                }
                super.handleMessage(message);
            }
        };
    }

    public ImageSaveLedView(Context context, AttributeSet attributeSet, int i) {
        super(context, attributeSet, i);
        this.widthCount = 5;
        this.heightCount = 10;
        this.pointMargin = 1;
        this.selectedColor3 = Color.rgb(82, 227, 255);
        this.selectedColor2 = Color.argb(Opcodes.IF_ACMPNE, 82, 227, 255);
        this.selectedColor1 = Color.argb(89, 82, 227, 255);
        this.selectedColorGray1 = Color.argb(89, 42, 122, 142);
        this.selectedColorGray2 = Color.argb(Opcodes.IF_ACMPNE, 42, 122, 142);
        this.selectedColorGray3 = Color.rgb(42, 122, 142);
        this.unSelectedColor = -1;
        this.timeCount = 0;
        this.leftMoveList = new LinkedList<>();
        this.rightMoveList = new LinkedList<>();
        this.topMoveRightList = new LinkedList<>();
        this.topMoveLeftList = new LinkedList<>();
        this.handler = new Handler() { // from class: cn.com.heaton.shiningmask.ui.widget.ImageSaveLedView.1
            @Override // android.os.Handler
            public void handleMessage(Message message) {
                if (message.what == 1) {
                    ImageSaveLedView.this.topAnimRight();
                } else if (message.what == 2) {
                    ImageSaveLedView.this.textLeftMove();
                } else if (message.what == 3) {
                    ImageSaveLedView.this.topAnimLeft();
                } else if (message.what == 4) {
                    ImageSaveLedView.this.textRightMove();
                }
                super.handleMessage(message);
            }
        };
    }

    public void init(int i, int i2) {
        this.widthCount = i;
        this.heightCount = i2;
        for (int i3 = 0; i3 < i * i2; i3++) {
            ImageSaveLedItemView imageSaveLedItemView = new ImageSaveLedItemView(getContext());
            imageSaveLedItemView.setViewNumber(i3);
            imageSaveLedItemView.setColumnNumber(i3 / i2);
            imageSaveLedItemView.setRowNumber(i3 % i2);
            imageSaveLedItemView.setPaint(this.unSelectedColor);
            imageSaveLedItemView.postInvalidate();
            addView(imageSaveLedItemView);
        }
    }

    public void init(int i, int i2, float f) {
        this.widthCount = i;
        this.heightCount = i2;
        for (int i3 = 0; i3 < i * i2; i3++) {
            ImageSaveLedItemView imageSaveLedItemView = new ImageSaveLedItemView(getContext());
            imageSaveLedItemView.setViewNumber(i3);
            imageSaveLedItemView.setColumnNumber(i3 / i2);
            imageSaveLedItemView.setRowNumber(i3 % i2);
            imageSaveLedItemView.setPaint(this.unSelectedColor, f);
            imageSaveLedItemView.postInvalidate();
            addView(imageSaveLedItemView);
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
        int i3 = this.widthSize;
        int i4 = this.widthCount;
        int i5 = i3 % i4;
        this.xMore = i5;
        int i6 = this.heightCount;
        this.yMore = size % i6;
        int i7 = i3 / i4;
        this.pointAllLength = i7;
        int i8 = size / i6;
        this.pointYAllLength = i8;
        int i9 = this.pointMargin;
        this.pointLength = i7 - (i9 * 2);
        int i10 = i5 / 2;
        this.offset = i10;
        this.moveMax = (i7 * i6) - size;
        this.moveYMax = (i8 * i4) - i3;
        int i11 = (i7 * i6) - (i9 + i10);
        this.heightSize = i11;
        setMeasuredDimension(i3, i11);
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

    public void clearSelected() {
        for (int i = 0; i < getChildCount(); i++) {
            ImageSaveLedItemView imageSaveLedItemView = (ImageSaveLedItemView) getChildAt(i);
            imageSaveLedItemView.setChecked(false);
            imageSaveLedItemView.setPaint(this.unSelectedColor);
            imageSaveLedItemView.postInvalidate();
        }
    }

    public void setData(byte[] bArr) {
        if (bArr == null) {
            return;
        }
        int childCount = getChildCount();
        for (int i = 0; i < childCount; i++) {
            ImageSaveLedItemView imageSaveLedItemView = (ImageSaveLedItemView) getChildAt(i);
            if (bArr[i] == 1) {
                imageSaveLedItemView.setChecked(true);
                imageSaveLedItemView.setPaint(this.selectedColor3);
            } else {
                imageSaveLedItemView.setChecked(false);
                imageSaveLedItemView.setPaint(this.unSelectedColor);
            }
            imageSaveLedItemView.postInvalidate();
        }
    }

    public void setData(List<Integer> list) {
        if (list == null) {
            return;
        }
        getChildCount();
        for (int i = 0; i < list.size(); i++) {
            ImageSaveLedItemView imageSaveLedItemView = (ImageSaveLedItemView) getChildAt(i);
            LogUtil.d("====data:" + list.get(i));
            if (list.get(i).intValue() == 1) {
                imageSaveLedItemView.setChecked(true);
                imageSaveLedItemView.setPaint(this.selectedColor3);
            } else {
                imageSaveLedItemView.setChecked(false);
                imageSaveLedItemView.setPaint(this.unSelectedColor);
            }
            imageSaveLedItemView.postInvalidate();
        }
    }

    public void setTextData(byte[] bArr) {
        this.textData = bArr;
        clearSelected();
        int length = bArr.length / 2;
        int[] iArr = new int[length];
        for (int i = 0; i < bArr.length / 2; i++) {
            int i2 = i * 2;
            iArr[i] = ((bArr[i2] & UByte.MAX_VALUE) * 64) + ((bArr[i2 + 1] & UByte.MAX_VALUE) >> 2);
        }
        for (int i3 = 0; i3 < 24; i3++) {
            if (i3 < length) {
                for (int i4 = 0; i4 < 9; i4++) {
                    int i5 = (iArr[i3] >> (i4 + 5)) & 1;
                    ImageSaveLedItemView imageSaveLedItemView = getImageSaveLedItemView(i3, i4);
                    if (i5 == 1) {
                        imageSaveLedItemView.setChecked(true);
                        imageSaveLedItemView.setPaint(this.selectedColor3);
                    } else {
                        imageSaveLedItemView.setChecked(false);
                        imageSaveLedItemView.setPaint(this.unSelectedColor);
                    }
                    imageSaveLedItemView.postInvalidate();
                }
            }
        }
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void textLeftMove() {
        for (int i = 0; i < 24; i++) {
            if (i < this.leftMoveList.size()) {
                for (int i2 = 0; i2 < 9; i2++) {
                    int iIntValue = (this.leftMoveList.get(i).intValue() >> (i2 + 5)) & 1;
                    ImageSaveLedItemView imageSaveLedItemView = getImageSaveLedItemView(i, i2);
                    if (iIntValue == 1) {
                        imageSaveLedItemView.setChecked(true);
                        imageSaveLedItemView.setPaint(this.selectedColor3);
                    } else {
                        imageSaveLedItemView.setChecked(false);
                        imageSaveLedItemView.setPaint(this.unSelectedColor);
                    }
                    imageSaveLedItemView.postInvalidate();
                }
            } else {
                LogUtil.d("====j");
            }
        }
        if (this.leftMoveList.size() > 0) {
            int iIntValue2 = this.leftMoveList.get(0).intValue();
            this.leftMoveList.remove(0);
            this.leftMoveList.add(Integer.valueOf(iIntValue2));
        }
    }

    public void setTextRight(byte[] bArr) {
        this.textData = bArr;
        if (bArr != null) {
            clearSelected();
            this.rightMoveList.clear();
            cancelTimerTask2();
            Timer timer = this.timer;
            if (timer != null) {
                timer.cancel();
                this.timer.purge();
                this.timer = null;
            }
        }
        int i = 0;
        while (true) {
            byte[] bArr2 = this.textData;
            if (i >= bArr2.length / 2) {
                break;
            }
            int i2 = i * 2;
            this.rightMoveList.add(Integer.valueOf(((bArr2[i2] & UByte.MAX_VALUE) * 64) + ((bArr2[i2 + 1] & UByte.MAX_VALUE) >> 2)));
            i++;
        }
        for (int i3 = 0; i3 < 24; i3++) {
            this.rightMoveList.addFirst(0);
        }
        this.timer = new Timer();
        MyTimerTask4 myTimerTask4 = new MyTimerTask4();
        this.myTimerTask4 = myTimerTask4;
        this.timer.schedule(myTimerTask4, 100L, 100L);
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void textRightMove() {
        for (int i = 23; i >= 0; i--) {
            if (i < this.rightMoveList.size()) {
                for (int i2 = 0; i2 < 9; i2++) {
                    int iIntValue = (this.rightMoveList.get(i).intValue() >> (i2 + 5)) & 1;
                    ImageSaveLedItemView imageSaveLedItemView = getImageSaveLedItemView(i, i2);
                    if (iIntValue == 1) {
                        imageSaveLedItemView.setChecked(true);
                        imageSaveLedItemView.setPaint(this.selectedColor3);
                    } else {
                        imageSaveLedItemView.setChecked(false);
                        imageSaveLedItemView.setPaint(this.unSelectedColor);
                    }
                    imageSaveLedItemView.postInvalidate();
                }
            } else {
                LogUtil.d("====j");
            }
        }
        if (this.rightMoveList.size() > 0) {
            int size = this.rightMoveList.size() - 1;
            int iIntValue2 = this.rightMoveList.get(size).intValue();
            this.rightMoveList.remove(size);
            this.rightMoveList.addFirst(Integer.valueOf(iIntValue2));
        }
    }

    private void textUpDownRightMove(List<Integer> list) {
        for (int i = 23; i >= 0; i--) {
            if (i < list.size()) {
                for (int i2 = 0; i2 < 9; i2++) {
                    int iIntValue = (list.get(i).intValue() >> (i2 + 5)) & 1;
                    ImageSaveLedItemView imageSaveLedItemView = getImageSaveLedItemView(i, i2);
                    if (iIntValue == 1) {
                        imageSaveLedItemView.setChecked(true);
                        imageSaveLedItemView.setPaint(this.selectedColor3);
                    } else {
                        imageSaveLedItemView.setChecked(false);
                        imageSaveLedItemView.setPaint(this.unSelectedColor);
                    }
                    imageSaveLedItemView.postInvalidate();
                }
            }
        }
    }

    private void textUpDownLeft(List<Integer> list) {
        for (int i = 0; i < 24; i++) {
            if (i < list.size()) {
                for (int i2 = 0; i2 < 9; i2++) {
                    int iIntValue = (list.get(i).intValue() >> (i2 + 5)) & 1;
                    ImageSaveLedItemView imageSaveLedItemView = getImageSaveLedItemView(i, i2);
                    if (iIntValue == 1) {
                        imageSaveLedItemView.setChecked(true);
                        imageSaveLedItemView.setPaint(this.selectedColor3);
                    } else {
                        imageSaveLedItemView.setChecked(false);
                        imageSaveLedItemView.setPaint(this.unSelectedColor);
                    }
                    imageSaveLedItemView.postInvalidate();
                }
            }
        }
    }

    public void setTextTopRightMove(byte[] bArr) {
        this.textData = bArr;
        if (bArr != null) {
            clearSelected();
            this.topMoveRightList.clear();
        }
        int i = 0;
        while (true) {
            byte[] bArr2 = this.textData;
            if (i >= bArr2.length / 2) {
                break;
            }
            int i2 = i * 2;
            this.topMoveRightList.add(Integer.valueOf(((bArr2[i2] & UByte.MAX_VALUE) * 64) + ((bArr2[i2 + 1] & UByte.MAX_VALUE) >> 2)));
            i++;
        }
        if (this.topMoveRightList.size() > 0) {
            for (int i3 = 0; i3 < 24; i3++) {
                this.topMoveRightList.addFirst(0);
            }
        }
        cancelTimerTask3();
        Timer timer = this.timer;
        if (timer != null) {
            timer.cancel();
            this.timer.purge();
            this.timer = null;
        }
        this.timer = new Timer();
        MyTimerTask1 myTimerTask1 = new MyTimerTask1();
        this.myTimerTask1 = myTimerTask1;
        this.timer.schedule(myTimerTask1, 100L, 100L);
    }

    public void setTextTopLeftMove(byte[] bArr) {
        this.textData = bArr;
        if (bArr != null) {
            clearSelected();
            this.topMoveLeftList.clear();
        }
        int i = 0;
        while (true) {
            byte[] bArr2 = this.textData;
            if (i >= bArr2.length / 2) {
                break;
            }
            int i2 = i * 2;
            this.topMoveLeftList.add(Integer.valueOf(((bArr2[i2] & UByte.MAX_VALUE) * 64) + ((bArr2[i2 + 1] & UByte.MAX_VALUE) >> 2)));
            i++;
        }
        if (this.topMoveLeftList.size() > 0) {
            for (int i3 = 0; i3 < 24; i3++) {
                this.topMoveLeftList.addFirst(0);
            }
        }
        cancelTimerTask1();
        Timer timer = this.timer;
        if (timer != null) {
            timer.cancel();
            this.timer.purge();
            this.timer = null;
        }
        this.timer = new Timer();
        MyTimerTask3 myTimerTask3 = new MyTimerTask3();
        this.myTimerTask3 = myTimerTask3;
        this.timer.schedule(myTimerTask3, 100L, 100L);
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void topAnimLeft() {
        int i = this.timeCount % 8;
        LinkedList linkedList = new LinkedList();
        LinkedList<Integer> linkedList2 = new LinkedList<>();
        for (int i2 = 1; i2 < this.topMoveLeftList.size(); i2++) {
            int iIntValue = this.topMoveLeftList.get(i2).intValue() << 6;
            if (this.timeCount % 1 == 0) {
                if (!this.up) {
                    iIntValue = i != 0 ? iIntValue >> i : iIntValue >> 8;
                } else if (i != 0) {
                    iIntValue >>= 8 - i;
                }
            }
            linkedList.add(Integer.valueOf(iIntValue));
            linkedList2.add(this.topMoveLeftList.get(i2));
        }
        int iIntValue2 = (this.topMoveLeftList.size() > 0 ? this.topMoveLeftList.get(0).intValue() : 0) << 6;
        if (this.timeCount % 1 == 0) {
            if (!this.up) {
                iIntValue2 = i != 0 ? iIntValue2 >> i : iIntValue2 >> 8;
            } else if (i != 0) {
                iIntValue2 >>= 8 - i;
            }
        }
        linkedList.add(Integer.valueOf(iIntValue2));
        if (this.topMoveLeftList.size() > 0) {
            linkedList2.add(this.topMoveLeftList.get(0));
        }
        this.topMoveLeftList = linkedList2;
        textUpDownLeft(linkedList);
        if (i == 0) {
            this.up = !this.up;
        }
        this.timeCount++;
    }

    /* JADX INFO: Access modifiers changed from: private */
    /*  JADX ERROR: NullPointerException in pass: LoopRegionVisitor
        java.lang.NullPointerException: Cannot invoke "jadx.core.dex.instructions.args.SSAVar.use(jadx.core.dex.instructions.args.RegisterArg)" because "ssaVar" is null
        	at jadx.core.dex.nodes.InsnNode.rebindArgs(InsnNode.java:506)
        	at jadx.core.dex.nodes.InsnNode.rebindArgs(InsnNode.java:509)
        */
    public void topAnimRight() {
        /*
            r6 = this;
            int r0 = r6.timeCount
            int r0 = r0 % 8
            java.util.LinkedList r1 = new java.util.LinkedList
            r1.<init>()
            java.util.LinkedList r2 = new java.util.LinkedList
            r2.<init>()
            java.util.LinkedList<java.lang.Integer> r3 = r6.topMoveRightList
            int r3 = r3.size()
            r4 = 0
            if (r3 <= 0) goto L2a
            java.util.LinkedList<java.lang.Integer> r3 = r6.topMoveRightList
            int r5 = r3.size()
            int r5 = r5 + (-1)
            java.lang.Object r3 = r3.get(r5)
            java.lang.Integer r3 = (java.lang.Integer) r3
            int r3 = r3.intValue()
            goto L2b
        L2a:
            r3 = r4
        L2b:
            int r3 = r3 << 6
            int r5 = r6.timeCount
            int r5 = r5 % 1
            if (r5 != 0) goto L43
            boolean r5 = r6.up
            if (r5 == 0) goto L3d
            if (r0 == 0) goto L43
            int r5 = 8 - r0
            int r3 = r3 >> r5
            goto L43
        L3d:
            if (r0 == 0) goto L41
            int r3 = r3 >> r0
            goto L43
        L41:
            int r3 = r3 >> 8
        L43:
            java.lang.Integer r3 = java.lang.Integer.valueOf(r3)
            r1.add(r3)
            java.util.LinkedList<java.lang.Integer> r3 = r6.topMoveRightList
            int r3 = r3.size()
            if (r3 <= 0) goto L63
            java.util.LinkedList<java.lang.Integer> r3 = r6.topMoveRightList
            int r5 = r3.size()
            int r5 = r5 + (-1)
            java.lang.Object r3 = r3.get(r5)
            java.lang.Integer r3 = (java.lang.Integer) r3
            r2.add(r3)
        L63:
            java.util.LinkedList<java.lang.Integer> r3 = r6.topMoveRightList
            int r3 = r3.size()
            int r3 = r3 + (-1)
            if (r4 >= r3) goto La6
            java.util.LinkedList<java.lang.Integer> r3 = r6.topMoveRightList
            java.lang.Object r3 = r3.get(r4)
            java.lang.Integer r3 = (java.lang.Integer) r3
            int r3 = r3.intValue()
            int r3 = r3 << 6
            int r5 = r6.timeCount
            int r5 = r5 % 1
            if (r5 != 0) goto L91
            boolean r5 = r6.up
            if (r5 == 0) goto L8b
            if (r0 == 0) goto L91
            int r5 = 8 - r0
            int r3 = r3 >> r5
            goto L91
        L8b:
            if (r0 == 0) goto L8f
            int r3 = r3 >> r0
            goto L91
        L8f:
            int r3 = r3 >> 8
        L91:
            java.lang.Integer r3 = java.lang.Integer.valueOf(r3)
            r1.add(r3)
            java.util.LinkedList<java.lang.Integer> r3 = r6.topMoveRightList
            java.lang.Object r3 = r3.get(r4)
            java.lang.Integer r3 = (java.lang.Integer) r3
            r2.add(r3)
            int r4 = r4 + 1
            goto L63
        La6:
            r6.topMoveRightList = r2
            r6.textUpDownRightMove(r1)
            if (r0 != 0) goto Lb3
            boolean r0 = r6.up
            r0 = r0 ^ 1
            r6.up = r0
        Lb3:
            int r0 = r6.timeCount
            int r0 = r0 + 1
            r6.timeCount = r0
            return
        */
        throw new UnsupportedOperationException("Method not decompiled: cn.com.heaton.shiningmask.ui.widget.ImageSaveLedView.topAnimRight():void");
    }

    private ImageSaveLedItemView getImageSaveLedItemView(int i, int i2) {
        int i3 = (i * 12) + i2;
        LogUtil.d("index:" + i3);
        return (ImageSaveLedItemView) getChildAt(i3);
    }

    public void setImageData(int[] iArr) {
        this.imageData = iArr;
        clearSelected();
        int i = 0;
        for (int i2 = 0; i2 < 36; i2++) {
            for (int i3 = 0; i3 < 12; i3++) {
                ImageSaveLedItemView imageSaveLedItemView = getImageSaveLedItemView(i2, i3);
                int i4 = iArr[i];
                if (imageSaveLedItemView != null) {
                    imageSaveLedItemView.setChecked(true);
                    LogUtil.d("色值：" + i4 + "index:" + i);
                    int i5 = i4 & 255;
                    int i6 = (i4 >> 8) & 255;
                    int i7 = (i4 >> 16) & 255;
                    LogUtil.d("r:" + i5 + " g:" + i6 + " b:" + i7);
                    if (i5 == 0 && i6 == 0 && i7 == 0) {
                        imageSaveLedItemView.setPaint(0);
                    } else {
                        imageSaveLedItemView.setPaint(Color.rgb(i5, i6, i7));
                    }
                    imageSaveLedItemView.postInvalidate();
                }
                i++;
            }
        }
    }

    public void setSelectImageData(int[] iArr) {
        this.imageData = iArr;
        clearSelected();
        for (int i = 0; i < this.imageData.length; i++) {
            for (int i2 = 0; i2 < 9; i2++) {
                int i3 = (this.imageData[i] >> (i2 * 2)) & 3;
                ImageSaveLedItemView imageSaveLedItemView = getImageSaveLedItemView(i, i2);
                if (imageSaveLedItemView != null) {
                    if (i3 == 1) {
                        imageSaveLedItemView.setChecked(true);
                        imageSaveLedItemView.setPaint(this.selectedColor1);
                    } else if (i3 == 2) {
                        imageSaveLedItemView.setChecked(true);
                        imageSaveLedItemView.setPaint(this.selectedColor2);
                    } else if (i3 == 3) {
                        imageSaveLedItemView.setChecked(true);
                        imageSaveLedItemView.setPaint(this.selectedColor3);
                    } else {
                        imageSaveLedItemView.setChecked(false);
                        imageSaveLedItemView.setPaint(this.unSelectedColor);
                    }
                    imageSaveLedItemView.postInvalidate();
                }
            }
        }
    }

    public void setPointMargin(int i) {
        this.pointMargin = i;
    }

    public void setSelectedColor(int i) {
        this.selectedColor3 = i;
    }

    public void updateUI() {
        byte[] bArr = this.textData;
        if (bArr != null) {
            setTextData(bArr);
        }
    }

    public int getOrientation() {
        return this.orientation;
    }

    public void setOrientation(int i) {
        this.orientation = i;
    }

    class MyTimerTask1 extends TimerTask {
        MyTimerTask1() {
        }

        @Override // java.util.TimerTask, java.lang.Runnable
        public void run() {
            Message message = new Message();
            message.what = 1;
            ImageSaveLedView.this.handler.sendMessage(message);
        }
    }

    class MyTimerTask2 extends TimerTask {
        MyTimerTask2() {
        }

        @Override // java.util.TimerTask, java.lang.Runnable
        public void run() {
            Message message = new Message();
            message.what = 2;
            ImageSaveLedView.this.handler.sendMessage(message);
        }
    }

    class MyTimerTask4 extends TimerTask {
        MyTimerTask4() {
        }

        @Override // java.util.TimerTask, java.lang.Runnable
        public void run() {
            Message message = new Message();
            message.what = 4;
            ImageSaveLedView.this.handler.sendMessage(message);
        }
    }

    class MyTimerTask3 extends TimerTask {
        MyTimerTask3() {
        }

        @Override // java.util.TimerTask, java.lang.Runnable
        public void run() {
            Message message = new Message();
            message.what = 3;
            ImageSaveLedView.this.handler.sendMessage(message);
        }
    }

    public void cancelTimerTask() {
        MyTimerTask1 myTimerTask1 = this.myTimerTask1;
        if (myTimerTask1 != null) {
            myTimerTask1.cancel();
            this.handler.removeCallbacks(this.myTimerTask1);
            this.myTimerTask1 = null;
        }
        MyTimerTask2 myTimerTask2 = this.myTimerTask2;
        if (myTimerTask2 != null) {
            myTimerTask2.cancel();
            this.handler.removeCallbacks(this.myTimerTask2);
            this.myTimerTask2 = null;
        }
        MyTimerTask3 myTimerTask3 = this.myTimerTask3;
        if (myTimerTask3 != null) {
            myTimerTask3.cancel();
            this.handler.removeCallbacks(this.myTimerTask3);
            this.myTimerTask3 = null;
        }
        MyTimerTask4 myTimerTask4 = this.myTimerTask4;
        if (myTimerTask4 != null) {
            myTimerTask4.cancel();
            this.handler.removeCallbacks(this.myTimerTask4);
            this.myTimerTask4 = null;
        }
        Timer timer = this.timer;
        if (timer != null) {
            timer.cancel();
            this.timer.purge();
            this.timer = null;
        }
    }

    public void cancelTimerTask4() {
        MyTimerTask4 myTimerTask4 = this.myTimerTask4;
        if (myTimerTask4 != null) {
            myTimerTask4.cancel();
            this.handler.removeCallbacks(this.myTimerTask4);
            this.myTimerTask4 = null;
        }
    }

    public void cancelTimerTask3() {
        MyTimerTask3 myTimerTask3 = this.myTimerTask3;
        if (myTimerTask3 != null) {
            myTimerTask3.cancel();
            this.handler.removeCallbacks(this.myTimerTask3);
            this.myTimerTask3 = null;
        }
    }

    public void cancelTimerTask1() {
        MyTimerTask1 myTimerTask1 = this.myTimerTask1;
        if (myTimerTask1 != null) {
            myTimerTask1.cancel();
            this.handler.removeCallbacks(this.myTimerTask1);
            this.myTimerTask1 = null;
        }
    }

    public void cancelTimerTask2() {
        MyTimerTask2 myTimerTask2 = this.myTimerTask2;
        if (myTimerTask2 != null) {
            myTimerTask2.cancel();
            this.handler.removeCallbacks(this.myTimerTask2);
            this.myTimerTask2 = null;
        }
    }

    public void setSaveDiyData(List<Integer> list, boolean z) {
        int childCount;
        if (list != null && list.size() == (childCount = getChildCount())) {
            for (int i = 0; i < childCount; i++) {
                ImageSaveLedItemView imageSaveLedItemView = (ImageSaveLedItemView) getChildAt(i);
                imageSaveLedItemView.setPaint(list.get(i).intValue());
                imageSaveLedItemView.postInvalidate();
            }
        }
    }
}