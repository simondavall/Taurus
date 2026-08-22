This document records planned enhancements, future ideas and technical debt. Items in this document represent potential future work rather than the current implementation priority. The backlog is intentionally broader than the current development roadmap.

# UI

# Features

# Enhancements
Set up releases for projects and replace the fixed in release text with drop down

# Technical Debt
- Move configuiration references to their own class. Verify all settings exist. 
E.g replace "Configuration.GetValue("Tickets:RequireFixedInReleaseForCompletion", true);" 
with something like Configuration.GetValue(Constants.Config.Tickets.RequireFixedInReleaseForCompletion, true);

# Nice-to-have
