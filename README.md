# Taurus

Taurus is a project and ticket management application for organising projects, tracking work and maintaining the information associated with tickets throughout their lifecycle.

It provides a central place for users to view projects, find and manage tickets, record progress, collaborate through comments and maintain relationships between pieces of work.

# Features

## Secure Access

Users sign in before accessing Taurus.

Access to the application is restricted to authenticated users, with clear handling when a user does not have permission to access the application.

## Project Management

Taurus provides a central list of projects.

Users can:

- View available projects.
- Create new projects.
- Edit existing projects.
- Delete projects.

Projects provide the organisational context for tickets within Taurus.

## Ticket Listing

Taurus provides a ticket listing for browsing and locating work.

Users can:

- View tickets across projects.
- Filter tickets by project.
- Apply predefined ticket filters.
- Identify ticket status and priority through visual indicators.
- Distinguish inactive tickets while retaining access to them.
- Open a ticket directly from the listing.

## Ticket Creation

Users can create new tickets within a project.

A ticket records the information required to describe and categorise a piece of work, including:

- Title.
- Description.
- Project.
- Type.
- Priority.
- Status.
- Parent ticket where applicable.

After creation, the ticket is available through its unique ticket reference.

## Ticket Details

Each ticket has a dedicated details page where users can view and maintain the ticket throughout its lifecycle.

The page presents the ticket's:

- Reference.
- Title.
- Description.
- Project.
- Type.
- Priority.
- Status.
- Fixed In Release information.
- Parent ticket where applicable.
- Assignment.
- Creation information.
- Last modification information.

Users can update editable ticket information directly from this page.

## Ticket Descriptions

Ticket descriptions support Markdown formatting.

Descriptions are displayed as formatted content while remaining editable as plain-text Markdown.

This allows descriptions to contain structured content such as paragraphs, lists, emphasis and links without requiring a rich-text editor.

## Comments

Tickets provide a comment history for recording discussion and additional information.

Users can:

- View existing comments.
- Add comments.
- Edit comments.
- Delete comments.
- Restore comments marked for deletion before saving their changes.

Comments support Markdown formatting and are displayed as formatted content.

## Ticket References

Descriptions and comments can reference other Taurus tickets using a simple ticket-reference notation.

For example:

`[ABC-123]`

When the referenced ticket exists, Taurus turns the reference into a link to that ticket.

References to tickets that do not exist remain as ordinary text.

This allows users to create useful connections between tickets naturally while writing descriptions and comments.

## Sub-Tasks

Tickets can contain sub-tasks.

Users can:

- Create a sub-task directly from its parent ticket.
- View the sub-tasks belonging to a ticket.
- See each sub-task's reference, title and age.
- Identify the status and priority of sub-tasks through visual indicators.
- Continue to access inactive sub-tasks.
- Open a sub-task directly from its parent.
- Navigate from a sub-task back to its parent ticket.

When a sub-task is created, Taurus takes the user directly to the newly created ticket.

## Ticket Assignment

Tickets can be assigned to a user.

The Assigned To field provides a list of available users ordered alphabetically by display name.

Users can:

- Assign an unassigned ticket.
- Reassign a ticket to another user.
- Unassign a ticket.

Unassigned tickets are clearly shown as `<Unassigned>`.

## Ticket Audit Information

Taurus displays information about the creation and most recent modification of a ticket.

This includes:

- Created By.
- Created date and time.
- Last Modified By.
- Last Modified date and time.

User names are displayed rather than internal user identifiers.

## Ticket Status and Completion

Tickets can progress through the available ticket statuses, including completion and obsolescence.

Taurus applies completion rules to prevent tickets from being closed when required conditions have not been met.

A ticket cannot be completed or made obsolete while it has active sub-tasks.

Taurus can also require a ticket to have a Fixed In Release value before it can be completed.

When a completion rule prevents an update, Taurus explains why the ticket cannot be closed so the user can resolve the outstanding requirement.

## Contextual Navigation

Taurus preserves the user's context while working between tickets.

For example:

- Opening a ticket from the ticket listing and closing it returns the user to the ticket listing.
- Opening a sub-task from its parent and closing it returns the user to the parent ticket.
- Moving through multiple levels of tickets preserves the corresponding navigation path.

This allows users to work through related tickets without repeatedly finding their way back to the place from which they started.

## Validation and Feedback

Taurus validates ticket and project information before accepting changes.

Validation is presented according to the type of problem:

- Problems associated with a particular field are shown with that field.
- Rules affecting the ticket as a whole are presented prominently to the user.

Users can dismiss general validation messages after reviewing them.

The application also provides clear feedback when operations succeed or when an operation cannot be completed.

## Responsive Use

Taurus is designed to support desktop, tablet and mobile screen sizes.

Core project and ticket workflows remain accessible as the available screen space changes, allowing users to work with Taurus across different device sizes.