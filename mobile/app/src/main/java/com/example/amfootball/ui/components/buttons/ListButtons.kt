package com.example.amfootball.ui.components.buttons

import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Check
import androidx.compose.material.icons.filled.Close
import androidx.compose.material.icons.filled.Email
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.runtime.Composable
import androidx.compose.ui.res.stringResource
import com.example.amfootball.R

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

@Composable
fun ListSendMemberShipRequestButton(sendMemberShipRequest: () -> Unit) {
    IconButton(onClick = sendMemberShipRequest) {
        Icon(
            imageVector = Icons.Default.Email,
            contentDescription = stringResource(id = R.string.button_send_membership_request_description),
            tint = MaterialTheme.colorScheme.primary
        )
    }
}
