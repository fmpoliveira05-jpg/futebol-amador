package com.example.amfootball.ui.components

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.material3.Button
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.res.stringResource
import com.example.amfootball.R

@Composable
fun LoadingPage(
    isLoading: Boolean,
    errorMsg: String?,
    retry: () -> Unit,
    content: @Composable () -> Unit,
) {
    if (isLoading) {
        Loading()
    }
    else if (errorMsg != null) {
        RetryButton(
            errorMsg = errorMsg,
            retry = retry
        )
    }
    else {
        content()
    }
}

@Composable
fun Loading() {
    Column(
        modifier = Modifier.fillMaxSize(),
        verticalArrangement = Arrangement.Center,
        horizontalAlignment = Alignment.CenterHorizontally
    ) {
        CircularProgressIndicator()
    }
}

@Composable
fun RetryButton(
    errorMsg: String?,
    retry: () -> Unit
) {
    Column {
        Text(text = errorMsg!!, color = Color.Red)
        Button(onClick = retry) {
            Text(text = stringResource(id = R.string.button_try_again))
        }
    }
}