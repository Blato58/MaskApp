package cn.com.heaton.shiningmask.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.RelativeLayout;
import androidx.viewbinding.ViewBinding;
import androidx.viewbinding.ViewBindings;
import cn.com.heaton.shiningmask.R;
import cn.com.heaton.shiningmask.ui.widget.ledaddview.LedAddView;

/* JADX INFO: loaded from: classes.dex */
public final class ItemLedviewBinding implements ViewBinding {
    public final LedAddView ltvPreview;
    public final RelativeLayout rootView;
    private final RelativeLayout rootView_;

    private ItemLedviewBinding(RelativeLayout relativeLayout, LedAddView ledAddView, RelativeLayout relativeLayout2) {
        this.rootView_ = relativeLayout;
        this.ltvPreview = ledAddView;
        this.rootView = relativeLayout2;
    }

    @Override // androidx.viewbinding.ViewBinding
    public RelativeLayout getRoot() {
        return this.rootView_;
    }

    public static ItemLedviewBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static ItemLedviewBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.item_ledview, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static ItemLedviewBinding bind(View view) {
        int i = R.id.ltv_preview;
        LedAddView ledAddView = (LedAddView) ViewBindings.findChildViewById(view, i);
        if (ledAddView != null) {
            RelativeLayout relativeLayout = (RelativeLayout) view;
            return new ItemLedviewBinding(relativeLayout, ledAddView, relativeLayout);
        }
        throw new NullPointerException("Missing required view with ID: ".concat(view.getResources().getResourceName(i)));
    }
}