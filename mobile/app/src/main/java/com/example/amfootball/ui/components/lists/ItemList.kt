package com.example.amfootball.ui.components.lists

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.CalendarMonth
import androidx.compose.material.icons.filled.Check
import androidx.compose.material.icons.filled.ChevronRight
import androidx.compose.material.icons.filled.Close
import androidx.compose.material.icons.filled.Star
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
import com.example.amfootball.utils.Patterns
import java.time.format.DateTimeFormatter

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
        IconButton(
            onClick = accept
        ) {
            Icon(
                imageVector = Icons.Default.Check,
                contentDescription = stringResource(id = R.string.accept_button_description),
                tint = MaterialTheme.colorScheme.primary
            )
        }

        IconButton(
            onClick = reject
        ) {
            Icon(
                imageVector = Icons.Default.Close,
                contentDescription = stringResource(id = R.string.reject_button_description),
                tint = MaterialTheme.colorScheme.error
            )
        }

        IconButton(
            onClick = showMore
        ) {
            Icon(
                imageVector = Icons.Default.ChevronRight,
                contentDescription = stringResource(id = R.string.list_teams_view_team),
                tint = MaterialTheme.colorScheme.outline
            )
        }
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