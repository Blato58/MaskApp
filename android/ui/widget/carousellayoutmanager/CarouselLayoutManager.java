package cn.com.heaton.shiningmask.ui.widget.carousellayoutmanager;

import android.graphics.PointF;
import android.os.Handler;
import android.os.Looper;
import android.os.Parcel;
import android.os.Parcelable;
import android.view.View;
import android.view.ViewGroup;
import androidx.core.view.ViewCompat;
import androidx.recyclerview.widget.RecyclerView;
import cn.com.heaton.shiningmask.ui.utils.LogUtil;
import java.lang.ref.WeakReference;
import java.util.ArrayList;
import java.util.Iterator;
import java.util.List;

/* JADX INFO: loaded from: classes.dex */
public class CarouselLayoutManager extends RecyclerView.LayoutManager {
    private static final boolean CIRCLE_LAYOUT = false;
    public static final int HORIZONTAL = 0;
    private static final int INVALID_POSITION = -1;
    public static final int MAX_VISIBLE_ITEMS = 2;
    public static final int VERTICAL = 1;
    private int j;
    private int mCenterItemPosition;
    private final boolean mCircleLayout;
    private Integer mDecoratedChildHeight;
    private Integer mDecoratedChildWidth;
    private int mFirstVisible1;
    private int mFirstVisible2;
    private int mItemsCount;
    private int mLastVisible1;
    private int mLastVisible2;
    private final LayoutHelper mLayoutHelper;
    private final List<OnCenterItemSelectionListener> mOnCenterItemSelectionListeners;
    private final int mOrientation;
    private CarouselSavedState mPendingCarouselSavedState;
    private int mPendingScrollPosition;
    private PostLayoutListener mViewPostLayout;

    public interface OnCenterItemSelectionListener {
        void onCenterItemChanged(int i);
    }

    public interface PostLayoutListener {
        ItemTransformation transformChild(View view, float f, int i);
    }

    public CarouselLayoutManager(int i) {
        this(i, false);
    }

    public CarouselLayoutManager(int i, boolean z) {
        this.mLastVisible1 = 0;
        this.mFirstVisible1 = 0;
        this.mLastVisible2 = 0;
        this.mFirstVisible2 = 0;
        this.mLayoutHelper = new LayoutHelper(2);
        this.mOnCenterItemSelectionListeners = new ArrayList();
        this.mCenterItemPosition = -1;
        if (i != 0 && 1 != i) {
            throw new IllegalArgumentException("orientation should be HORIZONTAL or VERTICAL");
        }
        this.mOrientation = i;
        this.mCircleLayout = z;
        this.mPendingScrollPosition = -1;
    }

    public void setPostLayoutListener(PostLayoutListener postLayoutListener) {
        this.mViewPostLayout = postLayoutListener;
        requestLayout();
    }

    public void setMaxVisibleItems(int i) {
        if (i <= 0) {
            throw new IllegalArgumentException("maxVisibleItems can't be less then 1");
        }
        this.mLayoutHelper.mMaxVisibleItems = i;
        requestLayout();
    }

    public int getMaxVisibleItems() {
        return this.mLayoutHelper.mMaxVisibleItems;
    }

    @Override // androidx.recyclerview.widget.RecyclerView.LayoutManager
    public RecyclerView.LayoutParams generateDefaultLayoutParams() {
        return new RecyclerView.LayoutParams(-2, -2);
    }

    public int getOrientation() {
        return this.mOrientation;
    }

    @Override // androidx.recyclerview.widget.RecyclerView.LayoutManager
    public boolean canScrollHorizontally() {
        return getChildCount() != 0 && this.mOrientation == 0;
    }

    @Override // androidx.recyclerview.widget.RecyclerView.LayoutManager
    public boolean canScrollVertically() {
        return getChildCount() != 0 && 1 == this.mOrientation;
    }

    public int getCenterItemPosition() {
        return this.mCenterItemPosition;
    }

    public void addOnItemSelectionListener(OnCenterItemSelectionListener onCenterItemSelectionListener) {
        this.mOnCenterItemSelectionListeners.add(onCenterItemSelectionListener);
    }

    public void removeOnItemSelectionListener(OnCenterItemSelectionListener onCenterItemSelectionListener) {
        this.mOnCenterItemSelectionListeners.remove(onCenterItemSelectionListener);
    }

    @Override // androidx.recyclerview.widget.RecyclerView.LayoutManager
    public void scrollToPosition(int i) {
        if (i < 0) {
            throw new IllegalArgumentException("position can't be less then 0. position is : " + i);
        }
        if (i >= this.mItemsCount) {
            throw new IllegalArgumentException("position can't be great then adapter items count. position is : " + i);
        }
        this.mPendingScrollPosition = i;
        requestLayout();
    }

    @Override // androidx.recyclerview.widget.RecyclerView.LayoutManager
    public void smoothScrollToPosition(RecyclerView recyclerView, RecyclerView.State state, int i) {
        LogUtil.d("smoothScrollToPosition:" + i);
        if (i < 0) {
            throw new IllegalArgumentException("position can't be less then 0. position is : " + i);
        }
        CarouselSmoothScroller carouselSmoothScroller = new CarouselSmoothScroller(recyclerView.getContext()) { // from class: cn.com.heaton.shiningmask.ui.widget.carousellayoutmanager.CarouselLayoutManager.1
            @Override // androidx.recyclerview.widget.RecyclerView.SmoothScroller
            public PointF computeScrollVectorForPosition(int i2) {
                return CarouselLayoutManager.this.computeScrollVectorForPosition(i2);
            }
        };
        carouselSmoothScroller.setTargetPosition(i);
        startSmoothScroll(carouselSmoothScroller);
        detectOnItemSelectionChanged(i, state);
    }

    protected PointF computeScrollVectorForPosition(int i) {
        if (getChildCount() == 0) {
            return null;
        }
        int i2 = ((float) i) < makeScrollPositionInRange0ToCount(getCurrentScrollPosition(), this.mItemsCount) ? -1 : 1;
        if (this.mOrientation == 0) {
            return new PointF(i2, 0.0f);
        }
        return new PointF(0.0f, i2);
    }

    @Override // androidx.recyclerview.widget.RecyclerView.LayoutManager
    public int scrollVerticallyBy(int i, RecyclerView.Recycler recycler, RecyclerView.State state) {
        if (this.mOrientation == 0) {
            return 0;
        }
        return scrollBy(i, recycler, state);
    }

    @Override // androidx.recyclerview.widget.RecyclerView.LayoutManager
    public int scrollHorizontallyBy(int i, RecyclerView.Recycler recycler, RecyclerView.State state) {
        if (1 == this.mOrientation) {
            return 0;
        }
        return scrollBy(i, recycler, state);
    }

    protected int scrollBy(int i, RecyclerView.Recycler recycler, RecyclerView.State state) {
        if (getChildCount() == 0 || i == 0) {
            return 0;
        }
        if (this.mCircleLayout) {
            this.mLayoutHelper.mScrollOffset += i;
            int scrollItemSize = getScrollItemSize() * this.mItemsCount;
            while (this.mLayoutHelper.mScrollOffset < 0) {
                this.mLayoutHelper.mScrollOffset += scrollItemSize;
            }
            while (this.mLayoutHelper.mScrollOffset > scrollItemSize) {
                this.mLayoutHelper.mScrollOffset -= scrollItemSize;
            }
            this.mLayoutHelper.mScrollOffset -= i;
        } else {
            int maxScrollOffset = getMaxScrollOffset();
            if (this.mLayoutHelper.mScrollOffset + i < 0) {
                i = -this.mLayoutHelper.mScrollOffset;
            } else if (this.mLayoutHelper.mScrollOffset + i > maxScrollOffset) {
                i = maxScrollOffset - this.mLayoutHelper.mScrollOffset;
            }
        }
        if (i != 0) {
            this.mLayoutHelper.mScrollOffset += i;
            fillData(recycler, state, false);
        }
        return i;
    }

    @Override // androidx.recyclerview.widget.RecyclerView.LayoutManager
    public void onMeasure(RecyclerView.Recycler recycler, RecyclerView.State state, int i, int i2) {
        this.mDecoratedChildHeight = null;
        this.mDecoratedChildWidth = null;
        super.onMeasure(recycler, state, i, i2);
    }

    @Override // androidx.recyclerview.widget.RecyclerView.LayoutManager
    public void onLayoutChildren(RecyclerView.Recycler recycler, RecyclerView.State state) {
        int i;
        LogUtil.d("选中的item1:-1");
        boolean z = false;
        if (this.mDecoratedChildWidth == null) {
            View viewForPosition = recycler.getViewForPosition(0);
            addView(viewForPosition);
            measureChildWithMargins(viewForPosition, 0, 0);
            this.mDecoratedChildWidth = Integer.valueOf(getDecoratedMeasuredWidth(viewForPosition));
            this.mDecoratedChildHeight = Integer.valueOf(getDecoratedMeasuredHeight(viewForPosition));
            removeAndRecycleView(viewForPosition, recycler);
            if (-1 == this.mPendingScrollPosition && this.mPendingCarouselSavedState == null) {
                this.mPendingScrollPosition = this.mCenterItemPosition;
            }
            z = true;
        }
        int i2 = this.mPendingScrollPosition;
        if (-1 != i2) {
            this.mLayoutHelper.mScrollOffset = calculateScrollForSelectingPosition(i2, state);
            this.mPendingScrollPosition = -1;
            this.mPendingCarouselSavedState = null;
        } else {
            CarouselSavedState carouselSavedState = this.mPendingCarouselSavedState;
            if (carouselSavedState != null) {
                this.mLayoutHelper.mScrollOffset = calculateScrollForSelectingPosition(carouselSavedState.mCenterItemPosition, state);
                this.mPendingCarouselSavedState = null;
            } else if (state.didStructureChange() && -1 != (i = this.mCenterItemPosition)) {
                this.mLayoutHelper.mScrollOffset = calculateScrollForSelectingPosition(i, state);
            }
        }
        fillData(recycler, state, z);
    }

    private int calculateScrollForSelectingPosition(int i, RecyclerView.State state) {
        if (i >= 5) {
            i = 4;
        }
        return i * (1 == this.mOrientation ? this.mDecoratedChildHeight : this.mDecoratedChildWidth).intValue();
    }

    private void fillData(RecyclerView.Recycler recycler, RecyclerView.State state, boolean z) {
        float currentScrollPosition = getCurrentScrollPosition();
        generateLayoutOrder(currentScrollPosition, state);
        removeAndRecycleUnusedViews(this.mLayoutHelper, recycler);
        int widthNoPadding = getWidthNoPadding();
        int heightNoPadding = getHeightNoPadding();
        if (1 == this.mOrientation) {
            fillDataVertical(recycler, widthNoPadding, heightNoPadding, z);
        } else {
            fillDataHorizontal(recycler, widthNoPadding, heightNoPadding, z);
        }
        recycler.clear();
        detectOnItemSelectionChanged(currentScrollPosition, state);
    }

    private void detectOnItemSelectionChanged(float f, RecyclerView.State state) {
        final int iRound = Math.round(makeScrollPositionInRange0ToCount(f, 5));
        if (this.mCenterItemPosition != iRound) {
            this.mCenterItemPosition = iRound;
            new Handler(Looper.getMainLooper()).post(new Runnable() { // from class: cn.com.heaton.shiningmask.ui.widget.carousellayoutmanager.CarouselLayoutManager.2
                @Override // java.lang.Runnable
                public void run() {
                    LogUtil.d("选中的item:" + iRound);
                    CarouselLayoutManager.this.selectItemCenterPosition(iRound);
                }
            });
        }
    }

    public void selectItemCenterPosition(int i) {
        Iterator<OnCenterItemSelectionListener> it = this.mOnCenterItemSelectionListeners.iterator();
        while (it.hasNext()) {
            it.next().onCenterItemChanged(i);
        }
    }

    private void fillDataVertical(RecyclerView.Recycler recycler, int i, int i2, boolean z) {
        int iIntValue = (i - this.mDecoratedChildWidth.intValue()) / 2;
        int iIntValue2 = iIntValue + this.mDecoratedChildWidth.intValue();
        int iIntValue3 = (i2 - this.mDecoratedChildHeight.intValue()) / 2;
        int length = this.mLayoutHelper.mLayoutOrder.length;
        for (int i3 = 0; i3 < length; i3++) {
            LayoutOrder layoutOrder = this.mLayoutHelper.mLayoutOrder[i3];
            int cardOffsetByPositionDiff = iIntValue3 + getCardOffsetByPositionDiff(layoutOrder.mItemPositionDiff);
            fillChildItem(iIntValue, cardOffsetByPositionDiff, iIntValue2, cardOffsetByPositionDiff + this.mDecoratedChildHeight.intValue(), layoutOrder, recycler, i3, z);
        }
    }

    private void fillDataHorizontal(RecyclerView.Recycler recycler, int i, int i2, boolean z) {
        int iIntValue = (i2 - this.mDecoratedChildHeight.intValue()) / 2;
        int iIntValue2 = iIntValue + this.mDecoratedChildHeight.intValue();
        int iIntValue3 = (i - this.mDecoratedChildWidth.intValue()) / 2;
        int length = this.mLayoutHelper.mLayoutOrder.length;
        for (int i3 = 0; i3 < length; i3++) {
            LayoutOrder layoutOrder = this.mLayoutHelper.mLayoutOrder[i3];
            int cardOffsetByPositionDiff = iIntValue3 + getCardOffsetByPositionDiff(layoutOrder.mItemPositionDiff);
            fillChildItem(cardOffsetByPositionDiff, iIntValue, cardOffsetByPositionDiff + this.mDecoratedChildWidth.intValue(), iIntValue2, layoutOrder, recycler, i3, z);
        }
    }

    private void removeAndRecycleUnusedViews(LayoutHelper layoutHelper, RecyclerView.Recycler recycler) {
        ArrayList arrayList = new ArrayList();
        int childCount = getChildCount();
        for (int i = 0; i < childCount; i++) {
            View childAt = getChildAt(i);
            ViewGroup.LayoutParams layoutParams = childAt.getLayoutParams();
            if (!(layoutParams instanceof RecyclerView.LayoutParams)) {
                arrayList.add(childAt);
            } else {
                RecyclerView.LayoutParams layoutParams2 = (RecyclerView.LayoutParams) layoutParams;
                int viewAdapterPosition = layoutParams2.getViewAdapterPosition();
                if (layoutParams2.isItemRemoved() || !layoutHelper.hasAdapterPosition(viewAdapterPosition)) {
                    arrayList.add(childAt);
                }
            }
        }
        Iterator it = arrayList.iterator();
        while (it.hasNext()) {
            removeAndRecycleView((View) it.next(), recycler);
        }
    }

    private void fillChildItem(int i, int i2, int i3, int i4, LayoutOrder layoutOrder, RecyclerView.Recycler recycler, int i5, boolean z) {
        View viewBindChild = bindChild(layoutOrder.mItemAdapterPosition, recycler, z);
        ViewCompat.setElevation(viewBindChild, i5);
        PostLayoutListener postLayoutListener = this.mViewPostLayout;
        ItemTransformation itemTransformationTransformChild = postLayoutListener != null ? postLayoutListener.transformChild(viewBindChild, layoutOrder.mItemPositionDiff, this.mOrientation) : null;
        if (itemTransformationTransformChild == null) {
            viewBindChild.layout(i, i2, i3, i4);
            return;
        }
        viewBindChild.layout(Math.round(i + itemTransformationTransformChild.mTranslationX), Math.round(i2 + itemTransformationTransformChild.mTranslationY), Math.round(i3 + itemTransformationTransformChild.mTranslationX), Math.round(i4 + itemTransformationTransformChild.mTranslationY));
        ViewCompat.setScaleX(viewBindChild, itemTransformationTransformChild.mScaleX);
        ViewCompat.setScaleY(viewBindChild, itemTransformationTransformChild.mScaleY);
    }

    private float getCurrentScrollPosition() {
        if (getMaxScrollOffset() == 0) {
            return 0.0f;
        }
        return (this.mLayoutHelper.mScrollOffset * 1.0f) / getScrollItemSize();
    }

    private int getMaxScrollOffset() {
        return getScrollItemSize() * (this.mItemsCount - 1);
    }

    private void generateLayoutOrder(float f, RecyclerView.State state) {
        int itemCount = state.getItemCount();
        this.mItemsCount = itemCount;
        float fMakeScrollPositionInRange0ToCount = makeScrollPositionInRange0ToCount(f, itemCount);
        int iRound = Math.round(fMakeScrollPositionInRange0ToCount);
        if (this.mCircleLayout && 1 < this.mItemsCount) {
            int iMin = Math.min((this.mLayoutHelper.mMaxVisibleItems * 2) + 3, this.mItemsCount);
            this.mLayoutHelper.initLayoutOrder(iMin);
            int i = iMin / 2;
            for (int i2 = 1; i2 <= i; i2++) {
                float f2 = i2;
                this.mLayoutHelper.setLayoutOrder(i - i2, Math.round((fMakeScrollPositionInRange0ToCount - f2) + this.mItemsCount) % this.mItemsCount, (iRound - fMakeScrollPositionInRange0ToCount) - f2);
            }
            int i3 = iMin - 1;
            for (int i4 = i3; i4 >= i + 1; i4--) {
                float f3 = i4;
                float f4 = iMin;
                this.mLayoutHelper.setLayoutOrder(i4 - 1, Math.round((fMakeScrollPositionInRange0ToCount - f3) + f4) % this.mItemsCount, ((iRound - fMakeScrollPositionInRange0ToCount) + f4) - f3);
            }
            this.mLayoutHelper.setLayoutOrder(i3, iRound, iRound - fMakeScrollPositionInRange0ToCount);
            return;
        }
        int iMax = Math.max((iRound - this.mLayoutHelper.mMaxVisibleItems) - 1, 0);
        int iMin2 = Math.min(this.mLayoutHelper.mMaxVisibleItems + iRound + 1, this.mItemsCount - 1);
        int i5 = iMin2 - iMax;
        int i6 = i5 + 1;
        this.mLayoutHelper.initLayoutOrder(i6);
        for (int i7 = iMax; i7 <= iMin2; i7++) {
            this.j = i7;
            if (i7 == iRound) {
                this.mLayoutHelper.setLayoutOrder(i5, i7, i7 - fMakeScrollPositionInRange0ToCount);
            } else if (i7 < iRound) {
                this.mLayoutHelper.setLayoutOrder(i7 - iMax, i7, i7 - fMakeScrollPositionInRange0ToCount);
            } else {
                this.mLayoutHelper.setLayoutOrder((i6 - (i7 - iRound)) - 1, i7, i7 - fMakeScrollPositionInRange0ToCount);
            }
        }
    }

    public int getWidthNoPadding() {
        return (getWidth() - getPaddingStart()) - getPaddingEnd();
    }

    public int getHeightNoPadding() {
        return (getHeight() - getPaddingEnd()) - getPaddingStart();
    }

    private View bindChild(int i, RecyclerView.Recycler recycler, boolean z) {
        View viewFindViewForPosition = findViewForPosition(recycler, i);
        int iRound = Math.round(makeScrollPositionInRange0ToCount(getCurrentScrollPosition(), this.mItemsCount));
        Math.max((iRound - this.mLayoutHelper.mMaxVisibleItems) - 1, 0);
        Math.min(iRound + this.mLayoutHelper.mMaxVisibleItems + 1, this.mItemsCount - 1);
        if (viewFindViewForPosition.getParent() == null) {
            addView(viewFindViewForPosition);
            measureChildWithMargins(viewFindViewForPosition, 0, 0);
            return viewFindViewForPosition;
        }
        if (z) {
            measureChildWithMargins(viewFindViewForPosition, 0, 0);
        }
        return viewFindViewForPosition;
    }

    private View findViewForPosition(RecyclerView.Recycler recycler, int i) {
        int childCount = getChildCount();
        for (int i2 = 0; i2 < childCount; i2++) {
            View childAt = getChildAt(i2);
            ViewGroup.LayoutParams layoutParams = childAt.getLayoutParams();
            if (layoutParams instanceof RecyclerView.LayoutParams) {
                RecyclerView.LayoutParams layoutParams2 = (RecyclerView.LayoutParams) layoutParams;
                if (layoutParams2.getViewAdapterPosition() == i) {
                    if (layoutParams2.isItemChanged()) {
                        recycler.bindViewToPosition(childAt, i);
                        measureChildWithMargins(childAt, 0, 0);
                    }
                    return childAt;
                }
            }
        }
        View viewForPosition = recycler.getViewForPosition(i);
        recycler.bindViewToPosition(viewForPosition, i);
        return viewForPosition;
    }

    protected int getCardOffsetByPositionDiff(float f) {
        int widthNoPadding;
        double dConvertItemPositionDiffToSmoothPositionDiff = convertItemPositionDiffToSmoothPositionDiff(f);
        if (1 == this.mOrientation) {
            widthNoPadding = (getHeightNoPadding() - this.mDecoratedChildHeight.intValue()) / 2;
        } else {
            widthNoPadding = (getWidthNoPadding() - this.mDecoratedChildWidth.intValue()) / 2;
        }
        return (int) Math.round(((double) (Math.signum(f) * widthNoPadding)) * dConvertItemPositionDiffToSmoothPositionDiff);
    }

    protected double convertItemPositionDiffToSmoothPositionDiff(float f) {
        double dAbs = Math.abs(f);
        if (dAbs > StrictMath.pow(1.0f / this.mLayoutHelper.mMaxVisibleItems, 0.3333333432674408d)) {
            return StrictMath.pow(r7 / this.mLayoutHelper.mMaxVisibleItems, 0.5d);
        }
        return StrictMath.pow(dAbs, 2.0d);
    }

    protected int getScrollItemSize() {
        if (1 == this.mOrientation) {
            return this.mDecoratedChildHeight.intValue();
        }
        return this.mDecoratedChildWidth.intValue();
    }

    @Override // androidx.recyclerview.widget.RecyclerView.LayoutManager
    public Parcelable onSaveInstanceState() {
        if (this.mPendingCarouselSavedState != null) {
            return new CarouselSavedState(this.mPendingCarouselSavedState);
        }
        CarouselSavedState carouselSavedState = new CarouselSavedState(super.onSaveInstanceState());
        carouselSavedState.mCenterItemPosition = this.mCenterItemPosition;
        return carouselSavedState;
    }

    @Override // androidx.recyclerview.widget.RecyclerView.LayoutManager
    public void onRestoreInstanceState(Parcelable parcelable) {
        if (parcelable instanceof CarouselSavedState) {
            CarouselSavedState carouselSavedState = (CarouselSavedState) parcelable;
            this.mPendingCarouselSavedState = carouselSavedState;
            super.onRestoreInstanceState(carouselSavedState.mSuperState);
            return;
        }
        super.onRestoreInstanceState(parcelable);
    }

    int getOffsetCenterView() {
        return (Math.round(getCurrentScrollPosition()) * getScrollItemSize()) - this.mLayoutHelper.mScrollOffset;
    }

    int getOffsetForCurrentView(View view) {
        int scrollItemSize;
        int position = getPosition(view);
        int scrollItemSize2 = (this.mLayoutHelper.mScrollOffset / (this.mItemsCount * getScrollItemSize())) * this.mItemsCount * getScrollItemSize();
        if (this.mLayoutHelper.mScrollOffset < 0) {
            scrollItemSize2--;
        }
        if (scrollItemSize2 == 0 || 0.0f < Math.signum(scrollItemSize2)) {
            scrollItemSize = this.mLayoutHelper.mScrollOffset - (position * getScrollItemSize());
        } else {
            scrollItemSize = this.mLayoutHelper.mScrollOffset + (position * getScrollItemSize());
        }
        return scrollItemSize - scrollItemSize2;
    }

    private static float makeScrollPositionInRange0ToCount(float f, int i) {
        while (0.0f > f) {
            f += i;
        }
        while (Math.round(f) >= i) {
            f -= i;
        }
        return f;
    }

    private static class LayoutHelper {
        private LayoutOrder[] mLayoutOrder;
        private int mMaxVisibleItems;
        private final List<WeakReference<LayoutOrder>> mReusedItems = new ArrayList();
        private int mScrollOffset;

        LayoutHelper(int i) {
            this.mMaxVisibleItems = i;
        }

        public void initLayoutOrder(int i) {
            LayoutOrder[] layoutOrderArr = this.mLayoutOrder;
            if (layoutOrderArr == null || layoutOrderArr.length != i) {
                if (layoutOrderArr != null) {
                    recycleItems(layoutOrderArr);
                }
                this.mLayoutOrder = new LayoutOrder[i];
                fillLayoutOrder();
            }
        }

        public void setLayoutOrder(int i, int i2, float f) {
            LayoutOrder layoutOrder = this.mLayoutOrder[i];
            layoutOrder.mItemAdapterPosition = i2;
            layoutOrder.mItemPositionDiff = f;
        }

        public boolean hasAdapterPosition(int i) {
            LayoutOrder[] layoutOrderArr = this.mLayoutOrder;
            if (layoutOrderArr != null) {
                for (LayoutOrder layoutOrder : layoutOrderArr) {
                    if (layoutOrder.mItemAdapterPosition == i) {
                        return true;
                    }
                }
            }
            return false;
        }

        private void recycleItems(LayoutOrder... layoutOrderArr) {
            for (LayoutOrder layoutOrder : layoutOrderArr) {
                this.mReusedItems.add(new WeakReference<>(layoutOrder));
            }
        }

        private void fillLayoutOrder() {
            int i = 0;
            while (true) {
                LayoutOrder[] layoutOrderArr = this.mLayoutOrder;
                if (i >= layoutOrderArr.length) {
                    return;
                }
                if (layoutOrderArr[i] == null) {
                    layoutOrderArr[i] = createLayoutOrder();
                }
                i++;
            }
        }

        private LayoutOrder createLayoutOrder() {
            Iterator<WeakReference<LayoutOrder>> it = this.mReusedItems.iterator();
            while (it.hasNext()) {
                LayoutOrder layoutOrder = it.next().get();
                it.remove();
                if (layoutOrder != null) {
                    return layoutOrder;
                }
            }
            return new LayoutOrder();
        }
    }

    private static class LayoutOrder {
        private int mItemAdapterPosition;
        private float mItemPositionDiff;

        private LayoutOrder() {
        }
    }

    public static class CarouselSavedState implements Parcelable {
        public static final Parcelable.Creator<CarouselSavedState> CREATOR = new Parcelable.Creator<CarouselSavedState>() { // from class: cn.com.heaton.shiningmask.ui.widget.carousellayoutmanager.CarouselLayoutManager.CarouselSavedState.1
            /* JADX WARN: Can't rename method to resolve collision */
            @Override // android.os.Parcelable.Creator
            public CarouselSavedState createFromParcel(Parcel parcel) {
                return new CarouselSavedState(parcel);
            }

            /* JADX WARN: Can't rename method to resolve collision */
            @Override // android.os.Parcelable.Creator
            public CarouselSavedState[] newArray(int i) {
                return new CarouselSavedState[i];
            }
        };
        private int mCenterItemPosition;
        private final Parcelable mSuperState;

        @Override // android.os.Parcelable
        public int describeContents() {
            return 0;
        }

        public CarouselSavedState(Parcelable parcelable) {
            this.mSuperState = parcelable;
        }

        private CarouselSavedState(Parcel parcel) {
            this.mSuperState = parcel.readParcelable(Parcelable.class.getClassLoader());
            this.mCenterItemPosition = parcel.readInt();
        }

        public CarouselSavedState(CarouselSavedState carouselSavedState) {
            this.mSuperState = carouselSavedState.mSuperState;
            this.mCenterItemPosition = carouselSavedState.mCenterItemPosition;
        }

        @Override // android.os.Parcelable
        public void writeToParcel(Parcel parcel, int i) {
            parcel.writeParcelable(this.mSuperState, i);
            parcel.writeInt(this.mCenterItemPosition);
        }
    }
}