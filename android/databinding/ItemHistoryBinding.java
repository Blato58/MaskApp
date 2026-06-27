package cn.com.heaton.shiningmask.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.LinearLayout;
import android.widget.RelativeLayout;
import androidx.viewbinding.ViewBinding;
import androidx.viewbinding.ViewBindings;
import cn.com.heaton.shiningmask.R;
import cn.com.heaton.shiningmask.ui.widget.LedTextView;

/* JADX INFO: loaded from: classes.dex */
public final class ItemHistoryBinding implements ViewBinding {
    public final LedTextView itemLedview;
    public final LinearLayout llLedView1;
    private final RelativeLayout rootView;

    private ItemHistoryBinding(RelativeLayout relativeLayout, LedTextView ledTextView, LinearLayout linearLayout) {
        this.rootView = relativeLayout;
        this.itemLedview = ledTextView;
        this.llLedView1 = linearLayout;
    }

    @Override // androidx.viewbinding.ViewBinding
    public RelativeLayout getRoot() {
        return this.rootView;
    }

    public static ItemHistoryBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static ItemHistoryBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.item_history, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static ItemHistoryBinding bind(View view) {
        int i = R.id.item_ledview;
        LedTextView ledTextView = (LedTextView) ViewBindings.findChildViewById(view, i);
        if (ledTextView != null) {
            i = R.id.ll_ledView1;
            LinearLayout linearLayout = (LinearLayout) ViewBindings.findChildViewById(view, i);
            if (linearLayout != null) {
                return new ItemHistoryBinding((RelativeLayout) view, ledTextView, linearLayout);
            }
        }
        throw new NullPointerException("Missing required view with ID: ".concat(view.getResources().getResourceName(i)));
    }
}