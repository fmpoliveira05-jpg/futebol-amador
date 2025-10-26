package com.example.amfootball.ui.components.NavBar
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.wrapContentHeight
import androidx.compose.foundation.layout.wrapContentSize
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ArrowBack
import androidx.compose.material3.Button
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.Text
import androidx.compose.material3.TopAppBar
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import androidx.compose.ui.window.Dialog
import androidx.compose.ui.window.DialogProperties
import androidx.compose.material3.Surface
@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun MyAppTopAppBar(topAppBarText: String, onBackPressed: () -> Unit) {
    TopAppBar(
        title = {
            Text(
                text = topAppBarText,
                textAlign = TextAlign.Center,
                modifier = Modifier
                    .fillMaxSize()
                    .wrapContentSize(Alignment.Center)
            )
        },
        navigationIcon = {
            IconButton(onClick = onBackPressed) {
                Icon(
                    Icons.AutoMirrored.Filled.ArrowBack,
                    contentDescription = "Test"
                )
            }
        }
    )
}
/*
@Composable
fun MyAlertDialog() {
    var dialogOpen by remember { mutableStateOf(false) }
    if (dialogOpen) {
        AlertDialog(
            onDismissRequest = { dialogOpen = false },
            buttons = {
                Row( modifier = Modifier.fillMaxWidth(),
                    horizontalArrangement = Arrangement.SpaceEvenly
                ) {
                    Button(onClick = { dialogOpen = false }) {
                        Text(text = "Confirm")
                    }
                    Button(onClick = { dialogOpen = false }) {
                        Text(text = "Dismiss")
                    }
                }
            },
            title = { Text(text = "My Title") },
            text = { Text(text = "My Desciption") },
            modifier = Modifier.fillMaxWidth().padding(28.dp),
            shape = RoundedCornerShape(10.dp),
            backgroundColor = Color.White,
            properties = DialogProperties(
                dismissOnBackPress = true,
                dismissOnClickOutside = true ) )
    }
    Button(onClick = { dialogOpen = true }) {
        Text(modifier = Modifier.fillMaxWidth(), text = "AlertDialog", textAlign = TextAlign.Center)
    }
}
*/


@Composable
fun MyCustomDialog() {
    var dialogOpen by remember { mutableStateOf(false) }
    if (dialogOpen) {
        Dialog( onDismissRequest = { dialogOpen = false },
            properties = DialogProperties(
                dismissOnBackPress = false,
                dismissOnClickOutside = false
            ) // test ci
        ) {
            Surface( modifier = Modifier.fillMaxWidth().wrapContentHeight(),
                shape = RoundedCornerShape(size = 10.dp)
            ) {
                Column( modifier = Modifier.padding(all = 16.dp).fillMaxWidth(),
                    verticalArrangement = Arrangement.Center,
                    horizontalAlignment = Alignment.CenterHorizontally
                ) { Text(text = "Custom Dialog UI Here")
                    Spacer(modifier = Modifier.padding(0.dp, 10.dp))
                    Button(onClick = { dialogOpen = false }) {
                        Text(text = "Ok") }
                }
            }
        }
    }
    Button(onClick = { dialogOpen = true }) {
        Text(
            modifier = Modifier.fillMaxWidth(),
            text = "CustomDialog",
            textAlign = TextAlign.Center
        )
    }
}