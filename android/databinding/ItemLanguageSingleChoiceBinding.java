package cn.com.heaton.shiningmask.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;
import androidx.viewbinding.ViewBinding;
import androidx.viewbinding.ViewBindings;
import cn.com.heaton.shiningmask.R;
import cn.com.heaton.shiningmask.ui.widget.CheckableLayout;

/* JADX INFO: loaded from: classes.dex */
public final class ItemLanguageSingleChoiceBinding implements ViewBinding {
    private final CheckableLayout rootView;
    public final TextView tvLanguageTitle;

    private ItemLanguageSingleChoiceBinding(CheckableLayout checkableLayout, TextView textView) {
        this.rootView = checkableLayout;
        this.tvLanguageTitle = textView;
    }

    @Override // androidx.viewbinding.ViewBinding
    public CheckableLayout getRoot() {
        return this.rootView;
    }

    public static ItemLanguageSingleChoiceBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static ItemLanguageSingleChoiceBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.item_language_single_choice, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static ItemLanguageSingleChoiceBinding bind(View view) {
        int i = R.id.tv_language_title;
        TextView textView = (TextView) ViewBindings.findChildViewById(view, i);
        if (textView != null) {
            return new ItemLanguageSingleChoiceBinding((CheckableLayout) view, textView);
        }
        throw new NullPointerException("Missing required view with ID: ".concat(view.getResources().getResourceName(i)));
    }
}