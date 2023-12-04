﻿using Discord.NET.SupportExtension.Core.Interface;
using Discord.NET.SupportExtension.MEF.CompletionSource;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.Mef.Commit {
    public class AsyncDiscordCompletionCommitManager : IAsyncCompletionCommitManager {
        public IEnumerable<char> PotentialCommitCharacters => ".".ToCharArray();

        public bool ShouldCommitCompletion(IAsyncCompletionSession session, SnapshotPoint location, char typedChar, CancellationToken token) {
            return false;
        }

        public CommitResult TryCommit(IAsyncCompletionSession session, ITextBuffer buffer, CompletionItem item, char typedChar, CancellationToken token) {
            if(item.Source is AsyncDiscordCompletionSource) {
                buffer.Replace(session.ApplicableToSpan.GetSpan(buffer.CurrentSnapshot), item.InsertText + $"/* {item.DisplayText} ({item.Suffix}) */");
                return CommitResult.Handled;
            }

            return CommitResult.Unhandled;
        }
    }
}
