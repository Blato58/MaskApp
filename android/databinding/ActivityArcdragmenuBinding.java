package cn.com.heaton.shiningmask.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.LinearLayout;
import androidx.viewbinding.ViewBinding;
import androidx.viewbinding.ViewBindings;
import cn.com.heaton.shiningmask.R;
import cn.com.heaton.shiningmask.ui.roteview.ArcDragMenu;

/* JADX INFO: loaded from: classes.dex */
public final class ActivityArcdragmenuBinding implements ViewBinding {
    public final ArcDragMenu arcdragmenu;
    private final LinearLayout rootView;

    private ActivityArcdragmenuBinding(LinearLayout linearLayout, ArcDragMenu arcDragMenu) {
        this.rootView = linearLayout;
        this.arcdragmenu = arcDragMenu;
    }

    @Override // androidx.viewbinding.ViewBinding
    public LinearLayout getRoot() {
        return this.rootView;
    }

    public static ActivityArcdragmenuBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static ActivityArcdragmenuBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.activity_arcdragmenu, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static ActivityArcdragmenuBinding bind(View view) {
        int i = R.id.arcdragmenu;
        ArcDragMenu arcDragMenu = (ArcDragMenu) ViewBindings.findChildViewById(view, i);
        if (arcDragMenu != null) {
            return new ActivityArcdragmenuBinding((LinearLayout) view, arcDragMenu);
        }
        throw new NullPointerException("Missing required view with ID: ".concat(view.getResources().getResourceName(i)));
    }
}