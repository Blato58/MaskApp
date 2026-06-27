package cn.com.heaton.shiningmask.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;
import androidx.viewbinding.ViewBinding;
import androidx.viewbinding.ViewBindings;
import cn.com.heaton.shiningmask.R;
import cn.com.heaton.shiningmask.ui.roteview.ArcMenu;

/* JADX INFO: loaded from: classes.dex */
public final class ArcMenuBinding implements ViewBinding {
    public final ArcMenu idArcmenu;
    public final TextView idButton;
    private final ArcMenu rootView;

    private ArcMenuBinding(ArcMenu arcMenu, ArcMenu arcMenu2, TextView textView) {
        this.rootView = arcMenu;
        this.idArcmenu = arcMenu2;
        this.idButton = textView;
    }

    @Override // androidx.viewbinding.ViewBinding
    public ArcMenu getRoot() {
        return this.rootView;
    }

    public static ArcMenuBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static ArcMenuBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.arc_menu, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static ArcMenuBinding bind(View view) {
        ArcMenu arcMenu = (ArcMenu) view;
        int i = R.id.id_button;
        TextView textView = (TextView) ViewBindings.findChildViewById(view, i);
        if (textView != null) {
            return new ArcMenuBinding(arcMenu, arcMenu, textView);
        }
        throw new NullPointerException("Missing required view with ID: ".concat(view.getResources().getResourceName(i)));
    }
}