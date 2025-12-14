package com.example.amfootball.ui.components.diaglos.pages

import androidx.compose.runtime.Composable
import androidx.compose.ui.res.stringResource
import com.example.amfootball.R
import com.example.amfootball.ui.components.diaglos.AppAlertDialog

@Composable
fun LeaveTeamAlertDialog(
    onLeaveTeam: () -> Unit,
    onDismiss: () -> Unit
) {
    AppAlertDialog(
        title = stringResource(id = R.string.dialog_title_leave_team),
        text = stringResource(id = R.string.dialog_message_leave_team),
        confirmButtonText = stringResource(id = R.string.dialog_confirm),
        dismissButtonText = stringResource(id = R.string.dialog_cancel),
        onConfirmClick = onLeaveTeam,
        onDismissClick = onDismiss
    )
}