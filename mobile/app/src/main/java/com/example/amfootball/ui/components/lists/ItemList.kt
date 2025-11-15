package com.example.amfootball.ui.components.lists

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.padding
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Check
import androidx.compose.material.icons.filled.ChevronRight
import androidx.compose.material.icons.filled.Close
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
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
fun<T> GenericPlayerListItem(
    item: T,
    title: (T) -> String,
    leading: @Composable (T) -> Unit = {},
    supporting: @Composable (T) -> Unit = {},
    trailing: @Composable () -> Unit = {},
) {
    ListItem(
        headlineContent = { //Conteudo Principal
            Text(
                text= title(item),
                style = MaterialTheme.typography.titleLarge,
                maxLines = 2,
                overflow = TextOverflow.Ellipsis
            )
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