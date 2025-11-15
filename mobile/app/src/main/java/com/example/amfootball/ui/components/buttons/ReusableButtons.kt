package com.example.amfootball.ui.components.buttons

import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Check
import androidx.compose.material.icons.filled.ChevronRight
import androidx.compose.material.icons.filled.Close
import androidx.compose.material.icons.filled.Send
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.runtime.Composable
import androidx.compose.ui.res.stringResource
import com.example.amfootball.R

@Composable
fun ShowMoreInfoButton(
    showMoreDetails: () -> Unit,
    contentDescription: String = stringResource(id = R.string.list_teams_view_team)
) {
    IconButton(
        onClick = { showMoreDetails() }
    ) {
        Icon(
            imageVector = Icons.Default.ChevronRight,
            contentDescription = contentDescription,
            tint = MaterialTheme.colorScheme.outline
        )
    }
}

@Composable
fun SendMemberShipRequestButton(
    onClickSendMembership: () -> Unit
) {
    TextButtonModel(
        onClick = { onClickSendMembership() },
        imageVector = Icons.Default.Send,
        contentDescription = "Send Membership Request",
        text = "Send Membership Request",
    )
}

@Composable
fun AcceptButton(accept: () -> Unit) {
    IconButton(
        onClick = accept
    ) {
        Icon(
            imageVector = Icons.Default.Check,
            contentDescription = stringResource(id = R.string.accept_button_description),
            tint = MaterialTheme.colorScheme.primary
        )
    }
}

@Composable
fun RejectButton(reject: () -> Unit) {
    IconButton(
        onClick = reject
    ) {
        Icon(
            imageVector = Icons.Default.Close,
            contentDescription = stringResource(id = R.string.reject_button_description),
            tint = MaterialTheme.colorScheme.error
        )
    }
}