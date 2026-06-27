package cn.com.heaton.shiningmask.ui.adapter;

import android.content.Context;
import cn.com.heaton.shiningmask.R;
import cn.com.heaton.shiningmask.ui.adapter.baseadapter.RecyclerAdapter;
import cn.com.heaton.shiningmask.ui.adapter.baseadapter.RecyclerViewHolder;
import java.util.List;

/* JADX INFO: loaded from: classes.dex */
public class MainMenuAdapter extends RecyclerAdapter<Integer> {
    private Context context;
    private OnItemClickListener onItemClickListener;

    interface OnItemClickListener {
        void onClick(int i);
    }

    public void setOniClickListener(OnItemClickListener onItemClickListener) {
        this.onItemClickListener = onItemClickListener;
    }

    @Override // cn.com.heaton.shiningmask.ui.adapter.baseadapter.RecyclerAdapter
    public void convert(RecyclerViewHolder recyclerViewHolder, Integer num) {
        recyclerViewHolder.setImageResource(R.id.iv_icon, num.intValue());
    }

    public MainMenuAdapter(Context context, int i, List<Integer> list) {
        super(context, i, list);
        this.context = context;
    }
}