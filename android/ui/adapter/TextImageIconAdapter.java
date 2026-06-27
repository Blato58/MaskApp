package cn.com.heaton.shiningmask.ui.adapter;

import android.content.Context;
import android.view.View;
import cn.com.heaton.shiningmask.R;
import cn.com.heaton.shiningmask.ui.adapter.baseadapter.RecyclerAdapter;
import cn.com.heaton.shiningmask.ui.adapter.baseadapter.RecyclerViewHolder;
import java.util.List;

/* JADX INFO: loaded from: classes.dex */
public class TextImageIconAdapter extends RecyclerAdapter<Integer> {
    private Context context;
    private OnItemClickListener onItemClickListener;

    public interface OnItemClickListener {
        void onClick(int i);
    }

    public void setOniClickListener(OnItemClickListener onItemClickListener) {
        this.onItemClickListener = onItemClickListener;
    }

    @Override // cn.com.heaton.shiningmask.ui.adapter.baseadapter.RecyclerAdapter
    public void convert(final RecyclerViewHolder recyclerViewHolder, Integer num) {
        recyclerViewHolder.setImageResource(R.id.iv_icon, num.intValue());
        recyclerViewHolder.setOnClickListener(R.id.iv_icon, new View.OnClickListener() { // from class: cn.com.heaton.shiningmask.ui.adapter.TextImageIconAdapter.1
            @Override // android.view.View.OnClickListener
            public void onClick(View view) {
                if (TextImageIconAdapter.this.onItemClickListener != null) {
                    TextImageIconAdapter.this.onItemClickListener.onClick(TextImageIconAdapter.this.getPosition(recyclerViewHolder));
                }
            }
        });
    }

    public TextImageIconAdapter(Context context, int i, List<Integer> list) {
        super(context, i, list);
        this.context = context;
    }
}