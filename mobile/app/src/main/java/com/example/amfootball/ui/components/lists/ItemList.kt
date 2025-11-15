package com.example.amfootball.ui.components.lists

import android.net.Uri
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.ElevatedCard
import androidx.compose.material3.ListItem
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.unit.dp
import com.example.amfootball.R
import com.example.amfootball.ui.components.buttons.AcceptButton
import com.example.amfootball.ui.components.buttons.RejectButton
import com.example.amfootball.ui.components.buttons.ShowMoreInfoButton

@Composable
fun ItemAcceptRejectAndShowMore(
    accept: () -> Unit,
    reject: () -> Unit,
    showMore: () -> Unit,
) {
    Row(
        verticalAlignment = Alignment.CenterVertically,
        horizontalArrangement = Arrangement.spacedBy(4.dp),
        modifier = Modifier.padding(start = 8.dp)
    ) {
        AcceptButton(accept = accept)

        RejectButton(reject = reject)

        ShowMoreInfoButton(
            showMoreDetails = showMore,
            contentDescription = stringResource(id = R.string.list_teams_view_team)
        )
    }
}


@Composable
fun<T> GenericListItem(
    item: T,
    title: (T) -> String,
    overline: @Composable () -> Unit = {},
    leading: @Composable (T) -> Unit = {},
    supporting: @Composable (T) -> Unit = {},
    trailing: @Composable () -> Unit = {},
    underneathItems: (@Composable () -> Unit)? = null
) {
    ElevatedCard(modifier = Modifier.fillMaxWidth()) {
        Column {
            ListItem(
                headlineContent = { //Conteudo Principal
                    Text(
                        text = title(item),
                        style = MaterialTheme.typography.titleLarge,
                        maxLines = 2,
                        overflow = TextOverflow.Ellipsis
                    )
                },
                overlineContent = {
                    overline()
                },
                supportingContent = {
                    supporting(item)
                },
                leadingContent = {
                    leading(item)
                },
                trailingContent = {
                    trailing()
                }
            )
        }

        if (underneathItems != null) {
            Row(
                verticalAlignment = Alignment.CenterVertically,
                horizontalArrangement = Arrangement.Center,
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(horizontal = 8.dp, vertical = 4.dp)
            ) {
                underneathItems()
            }
        }
    }
}